using Application.Admin.Commands.DeleteUserAdmin;
using Application.Common;
using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organization.Commands.DeleteDepartment;

public class DeleteDepartmentHandler(IKomSyncContext context, ICurrentUserService currentUser, IMediator mediator)
    : IRequestHandler<DeleteDepartmentCommand, bool>
{
    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        _ = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var dep = await context.Departments.FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (dep == null) return false;
        if (SystemAdminProtection.IsProtectedDepartment(dep))
            throw new BadRequestException("Системное подразделение нельзя удалить.");

        var userIds = await context.Users
            .Where(u => u.DepartmentId == dep.Id)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (userIds.Count > 0)
        {
            if (request.DeleteAllUsers)
            {
                foreach (var uid in userIds)
                    await mediator.Send(new DeleteUserAdminCommand(uid), cancellationToken);
            }
            else if (request.ReassignToDepartmentId.HasValue && request.PositionIdForReassignedUsers.HasValue)
            {
                if (await SystemAdminProtection.IsProtectedDepartmentIdAsync(
                        context, request.ReassignToDepartmentId.Value, cancellationToken))
                    throw new BadRequestException("Нельзя переносить сотрудников в системное подразделение.");
                if (await SystemAdminProtection.IsProtectedPositionIdAsync(
                        context, request.PositionIdForReassignedUsers.Value, cancellationToken))
                    throw new BadRequestException("Нельзя переносить сотрудников на системную должность.");

                if (request.ReassignToDepartmentId == dep.Id)
                    throw new BadRequestException("Выберите другое подразделение для переноса сотрудников.");

                var targetPos = await context.Positions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.PositionIdForReassignedUsers.Value, cancellationToken);
                if (targetPos == null || targetPos.DepartmentId != request.ReassignToDepartmentId.Value)
                    throw new BadRequestException("Указанная должность не принадлежит целевому подразделению.");

                await context.Users
                    .Where(u => u.DepartmentId == dep.Id)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(u => u.DepartmentId, _ => request.ReassignToDepartmentId!.Value)
                            .SetProperty(u => u.PositionId, _ => request.PositionIdForReassignedUsers!.Value),
                        cancellationToken);
            }
            else
            {
                throw new BadRequestException(
                    "В подразделении есть сотрудники: переназначьте их в другое подразделение и должность или удалите всех.");
            }
        }

        var projectsInDept = await context.Projects
            .Where(p => p.DepartmentId == dep.Id)
            .ToListAsync(cancellationToken);
        context.Projects.RemoveRange(projectsInDept);

        var positions = await context.Positions.Where(p => p.DepartmentId == dep.Id).ToListAsync(cancellationToken);
        foreach (var pos in positions)
        {
            if (await context.Users.AnyAsync(u => u.PositionId == pos.Id, cancellationToken))
                throw new BadRequestException("Нельзя удалить подразделение: у должностей этого отдела есть сотрудники.");
        }

        context.Positions.RemoveRange(positions);
        context.Departments.Remove(dep);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
