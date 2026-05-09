using Application.DTO.Attachments;
using Application.DTO.Projects;
using Application.DTO.TaskComment;
using Application.DTO.TaskHistory;
using Domain.Enums;

namespace Application.DTO.Tasks;

public class TaskDetailedDto
{
    public Guid Id { get; init; }
    public string Key { get; set; } = null!;
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public TaskStatusColumnDto Status { get; init; } = null!;
    public ProjectTaskPriority Priority { get; init; }
    public Guid ProjectId { get; init; }
    public Guid CreatorId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? Deadline { get; init; }
    public Guid? ParentTaskId { get; init; }
    public int TaskNumber { get; init; }
    public int SortOrder { get; init; }
    public TaskAssigneeDto? Assignee { get; init; }
    public TaskAssigneeDto? Responsible { get; init; }
    public TaskAssigneeDto? Creator { get; init; }
    public IReadOnlyList<TaskAssigneeDto> Watchers { get; set; } = Array.Empty<TaskAssigneeDto>();
    public TaskCommentDto[] Comments { get; set; } = [];
    public TaskHistoryDto[] History { get; set; } = [];
    public IReadOnlyList<FileAttachmentDto> FileAttachments { get; set; } = Array.Empty<FileAttachmentDto>();
    public IReadOnlyList<ProjectTagDto> Tags { get; set; } = Array.Empty<ProjectTagDto>();
}
