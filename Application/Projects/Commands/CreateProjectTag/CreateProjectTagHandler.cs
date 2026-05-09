using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Projects;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects.Commands.CreateProjectTag;

public class CreateProjectTagHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<CreateProjectTagCommand, ProjectTagDto>
{
    public async Task<ProjectTagDto> Handle(CreateProjectTagCommand request, CancellationToken cancellationToken)
    {
        if (!ProjectAccessRules.UserCanManageProjectsAndColumns(currentUser.Role))
            throw new ForbiddenException("Создавать теги могут только администратор или менеджер");

        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var project = await context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Проект не найден");

        if (!ProjectAccessRules.UserCanViewProject(currentUser.Role, uid, project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");

        var name = request.Name.Trim();
        if (name.Length == 0)
            throw new BadRequestException("Название тега не может быть пустым");

        var exists = await context.Tags.AnyAsync(
            t => t.ProjectId == request.ProjectId && t.Name.ToLower() == name.ToLower(),
            cancellationToken);
        if (exists)
            throw new BadRequestException("Тег с таким именем уже есть в проекте");

        var tag = new Tag { ProjectId = request.ProjectId, Name = name };
        context.Tags.Add(tag);
        await context.SaveChangesAsync(cancellationToken);

        return new ProjectTagDto(tag.Id, tag.Name);
    }
}
