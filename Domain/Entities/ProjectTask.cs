using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Interfaces;
using Domain.Enums;

namespace Domain.Entities;

public class ProjectTask : IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(500)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public Guid ProjectTaskStatusColumnId { get; set; }
    [ForeignKey(nameof(ProjectTaskStatusColumnId))]
    public ProjectTaskStatusColumn StatusColumn { get; set; } = null!;

    public ProjectTaskPriority Priority { get; set; } = ProjectTaskPriority.Medium;

    public DateTime? Deadline { get; set; }

    // --- Поля IAuditable (EF заполнит их сам в SaveChangesAsync) ---
    public DateTime CreatedAt { get; set; }
    public Guid CreatorId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? LastModifiedById { get; set; }

    // --- Связи ---
    public Guid ProjectId { get; set; }
    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;

    public Guid? ParentTaskId { get; set; }
    [ForeignKey(nameof(ParentTaskId))]
    public ProjectTask? ParentTask { get; set; }
    public ICollection<ProjectTask> SubTasks { get; set; } = new List<ProjectTask>();

    [ForeignKey(nameof(CreatorId))]
    public User Creator { get; set; } = null!;

    public Guid? AssigneeId { get; set; }
    [ForeignKey(nameof(AssigneeId))]
    public User? Assignee { get; set; }

    /// <summary>Ответственный за задачу (может отличаться от исполнителя).</summary>
    public Guid? ResponsibleId { get; set; }
    [ForeignKey(nameof(ResponsibleId))]
    public User? Responsible { get; set; }

    /// <summary>Порядковый номер задачи в рамках проекта (для ключа PROJ-12).</summary>
    public int TaskNumber { get; set; }

    /// <summary>Порядок карточки внутри колонки статуса.</summary>
    public int SortOrder { get; set; }

    public ICollection<ProjectTaskWatcher> Watchers { get; set; } = new List<ProjectTaskWatcher>();

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    public ICollection<DeadlineReminderLog> DeadlineReminderLogs { get; set; } = new List<DeadlineReminderLog>();

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}