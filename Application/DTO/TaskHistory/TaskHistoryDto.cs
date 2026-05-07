using Domain.Enums;

namespace Application.DTO.TaskHistory;

public record TaskHistoryDto
{
    public TaskHistoryDto(){}
    
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid? ChangedById { get; set; }
    public string? ChangedByName { get; set; }
    public bool ChangedByHasAvatar { get; set; }
    public string PropertyName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public TaskHistoryAction Action { get; set; }
    public DateTime ChangedAt { get; set; }
}