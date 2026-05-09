using Domain.Enums;
using MediatR;

namespace Application.DTO.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    Guid ProjectTaskStatusColumnId,
    ProjectTaskPriority Priority,
    Guid ProjectId,
    Guid? AssigneeId,
    Guid? ResponsibleId,
    DateTime? Deadline,
    IReadOnlyList<Guid>? WatcherUserIds,
    IReadOnlyList<Guid>? TagIds
) : IRequest<Guid>;
