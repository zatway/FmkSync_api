using Domain.Enums;
using MediatR;

namespace Application.DTO.Tasks;

public record UpdateTaskRequest(
    Guid Id,
    string? Title,
    Guid? ProjectTaskStatusColumnId,
    ProjectTaskPriority? Priority,
    DateTime? Deadline,
    string? Description,
    Guid ProjectId,
    Guid? ParentTaskId,
    Guid? ResponsibleId,
    int? SortOrder,
    IReadOnlyList<Guid>? WatcherUserIds,
    IReadOnlyList<Guid>? TagIds
) : IRequest<bool>;
