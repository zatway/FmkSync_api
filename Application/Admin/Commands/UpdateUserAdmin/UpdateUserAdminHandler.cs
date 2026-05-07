using Application.Common.Exceptions;
using Application.Common;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Commands.UpdateUserAdmin;

public class UpdateUserAdminHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser,
    IPasswordHasher passwordHasher)
    : IRequestHandler<UpdateUserAdminCommand, bool>
{
    public async Task<bool> Handle(UpdateUserAdminCommand request, CancellationToken cancellationToken)
    {
        _ = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return false;
        if (SystemAdminProtection.IsSystemAdminEmail(user.Email))
            throw new ForbiddenException("Системную учётную запись администратора нельзя изменять.");

        if (request.DepartmentId.HasValue
            && await SystemAdminProtection.IsProtectedDepartmentIdAsync(context, request.DepartmentId.Value, cancellationToken))
            throw new ForbiddenException("Нельзя назначать пользователей в системное подразделение.");

        if (request.PositionId.HasValue
            && await SystemAdminProtection.IsProtectedPositionIdAsync(context, request.PositionId.Value, cancellationToken))
            throw new ForbiddenException("Нельзя назначать пользователей на системную должность.");

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
            user.PasswordHash = passwordHasher.Hash(request.NewPassword.Trim());

        if (request.FullName != null) user.FullName = request.FullName.Trim();
        if (request.Email != null)
        {
            var email = request.Email.Trim();
            var exists = await context.Users
                .AnyAsync(u => u.NormalizedEmail == email.ToUpperInvariant() && u.Id != user.Id, cancellationToken);
            if (exists)
                throw new ConflictException("Этот email уже занят.");
            user.Email = email;
            user.NormalizedEmail = email.ToUpperInvariant();
        }
        if (request.IsApproved.HasValue) user.IsApproved = request.IsApproved.Value;
        if (request.Role.HasValue) user.Role = request.Role.Value;

        if (request.DepartmentId.HasValue) user.DepartmentId = request.DepartmentId.Value;
        if (request.PositionId.HasValue) user.PositionId = request.PositionId.Value;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

