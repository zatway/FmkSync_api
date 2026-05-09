using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Tasks;
using Application.Interfaces;
using Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tasks.Commands.CreateTask;

public class CreateTaskHandler(
    IKomSyncContext context,
    IMapper mapper,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateTaskRequest, Guid>
{
    public async Task<Guid> Handle(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
                     ?? throw new UnauthorizedAccessException("User must be logged in to create tasks.");

        if (!TaskAccessRules.UserCanCreateTasks(currentUserService.Role))
            throw new ForbiddenException("Недостаточно прав для создания задач");

        var project = await context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project == null)
            throw new NotFoundException("Проект не найден");
        if (!ProjectAccessRules.UserCanViewProject(
                currentUserService.Role, userId, project, currentUserService.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");

        var columnOk = await context.ProjectTaskStatusColumns
            .AnyAsync(c => c.Id == request.ProjectTaskStatusColumnId && c.ProjectId == request.ProjectId, cancellationToken);
        if (!columnOk)
            throw new BadRequestException("Некорректная колонка статуса для этого проекта");

        var task = mapper.Map<ProjectTask>(request);
        task.Id = Guid.NewGuid();
        task.CreatorId = userId;

        var maxNum = await context.Tasks
            .Where(t => t.ProjectId == request.ProjectId)
            .Select(t => (int?)t.TaskNumber)
            .MaxAsync(cancellationToken) ?? 0;

        task.TaskNumber = maxNum + 1;
        task.SortOrder = maxNum + 1;

        context.Tasks.Add(task);

        if (request.TagIds is { Count: > 0 })
        {
            var wanted = request.TagIds.Distinct().ToList();
            var tags = await context.Tags
                .Where(t => wanted.Contains(t.Id) && t.ProjectId == request.ProjectId)
                .ToListAsync(cancellationToken);
            if (tags.Count != wanted.Count)
                throw new BadRequestException("Указаны несуществующие теги или теги другого проекта");
            foreach (var t in tags)
                task.Tags.Add(t);
        }

        foreach (var watcherId in request.WatcherUserIds ?? Array.Empty<Guid>())
        {
            context.ProjectTaskWatchers.Add(new ProjectTaskWatcher
            {
                TaskId = task.Id,
                UserId = watcherId
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
