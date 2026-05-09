using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Tasks;
using Application.Interfaces;
using Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tasks.Commands.UpdateTask;

public class UpdateTaskHandler(
    IKomSyncContext context,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IRealtimeNotificationPublisher notifications
) : IRequestHandler<UpdateTaskRequest, bool>
{
    public async Task<bool> Handle(UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await context.Tasks
            .Include(t => t.Project)
            .ThenInclude(p => p.Members)
            .Include(t => t.Watchers)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (task == null)
            return false;

        var actorId = currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be logged in to update tasks.");

        if (task.ProjectId != request.ProjectId)
            return false;

        if (!ProjectAccessRules.UserCanViewProject(
                currentUserService.Role, actorId, task.Project, currentUserService.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");
        if (!TaskAccessRules.UserCanModifyTask(currentUserService.Role, actorId, task))
            throw new ForbiddenException("Недостаточно прав для изменения задачи");

        mapper.Map(request, task);
        task.UpdatedAt = DateTime.UtcNow;

        if (request.WatcherUserIds != null)
        {
            context.ProjectTaskWatchers.RemoveRange(task.Watchers);
            task.Watchers.Clear();

            foreach (var uid in request.WatcherUserIds)
            {
                var link = new ProjectTaskWatcher { TaskId = task.Id, UserId = uid };
                task.Watchers.Add(link);
                context.ProjectTaskWatchers.Add(link);
            }
        }

        if (request.TagIds != null)
        {
            var wanted = request.TagIds.Distinct().ToList();
            var tags = await context.Tags
                .Where(t => wanted.Contains(t.Id) && t.ProjectId == task.ProjectId)
                .ToListAsync(cancellationToken);
            if (tags.Count != wanted.Count)
                throw new BadRequestException("Указаны несуществующие теги или теги другого проекта");
            task.Tags.Clear();
            foreach (var t in tags)
                task.Tags.Add(t);
        }

        await context.SaveChangesAsync(cancellationToken);

        var recipients = new HashSet<Guid>();
        recipients.Add(task.CreatorId);
        if (task.AssigneeId.HasValue) recipients.Add(task.AssigneeId.Value);
        if (task.ResponsibleId.HasValue) recipients.Add(task.ResponsibleId.Value);
        foreach (var w in task.Watchers) recipients.Add(w.UserId);
        recipients.Remove(actorId);

        foreach (var recipientId in recipients)
        {
            await notifications.PublishToUserAsync(
                recipientId,
                "task.updated",
                new { taskId = task.Id, byUserId = actorId },
                cancellationToken);
        }

        return true;
    }
}
