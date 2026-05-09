using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Projects;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Projects.Commands.CreateProject
{
    public class CreateProjectHandler(IKomSyncContext context, IMapper mapper, ICurrentUserService currentUser)
        : IRequestHandler<CreateProjectRequest, Guid>
    {
        public async Task<Guid> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
        {
            if (currentUser.UserId == null)
                throw new UnauthorizedAccessException("User not authorized");
            if (!ProjectAccessRules.UserCanManageProjectsAndColumns(currentUser.Role))
                throw new ForbiddenException("Создавать проекты могут только администратор или менеджер");

            var project = mapper.Map<Project>(request);
            project.OwnerId = currentUser.UserId.Value;

            await context.Projects.AddAsync(project, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            if (request.Tags is { Count: > 0 })
            {
                ProjectTagsSync.SyncFromNames(project, request.Tags.ToList(), context);
                await context.SaveChangesAsync(cancellationToken);
            }

            return project.Id;
        }
    }
}