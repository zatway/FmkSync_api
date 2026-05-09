using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Projects;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects.Commands.UpdateProject
{
    public class UpdateProjectHandler(IKomSyncContext context, IMapper mapper, ICurrentUserService currentUser)
        : IRequestHandler<UpdateProjectRequest, bool>
    {
        public async Task<bool> Handle(UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
            var project = await context.Projects
                .Include(p => p.Members)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (project == null)
                throw new NotFoundException("Проект не найден");

            if (!ProjectAccessRules.UserCanViewProject(currentUser.Role, uid, project, currentUser.DepartmentId))
                throw new ForbiddenException("Нет доступа к проекту");
            if (!ProjectAccessRules.UserCanManageProjectsAndColumns(currentUser.Role))
                throw new ForbiddenException("Редактировать проект могут только администратор или менеджер");

            mapper.Map(request, project);
            project.UpdateTimestamp();
            ProjectTagsSync.SyncFromNames(project, request.Tags, context);

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}