using Application.Common;
using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects.Commands.DeleteProjectTag;

public class DeleteProjectTagHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<DeleteProjectTagCommand, bool>
{
    public async Task<bool> Handle(DeleteProjectTagCommand request, CancellationToken cancellationToken)
    {
        if (!ProjectAccessRules.UserCanManageProjectsAndColumns(currentUser.Role))
            throw new ForbiddenException("Удалять теги могут только администратор или менеджер");

        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var project = await context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project == null)
            return false;

        if (!ProjectAccessRules.UserCanViewProject(currentUser.Role, uid, project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");

        var tag = await context.Tags
            .FirstOrDefaultAsync(t => t.Id == request.TagId && t.ProjectId == request.ProjectId, cancellationToken);
        if (tag == null)
            return false;

        context.Tags.Remove(tag);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
