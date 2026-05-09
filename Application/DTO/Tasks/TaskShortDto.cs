using Application.DTO.Projects;
using Domain.Enums;

namespace Application.DTO.Tasks;

public record TaskShortDto
{
    public Guid Id { get; init; }
    public string Key { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public TaskStatusColumnDto Status { get; init; } = null!;
    public ProjectTaskPriority Priority { get; init; }
    public Guid ProjectId { get; init; }
    public Guid CreatorId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? Deadline { get; init; }
    public int TaskNumber { get; init; }
    public int SortOrder { get; init; }
    public TaskAssigneeDto? Assignee { get; init; }
    public TaskAssigneeDto? Responsible { get; init; }
    public IReadOnlyList<ProjectTagDto> Tags { get; init; } = Array.Empty<ProjectTagDto>();
}
