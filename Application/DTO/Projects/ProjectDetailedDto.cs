using Application.DTO.Attachments;

namespace Application.DTO.Projects;
public record ProjectDetailedDto(
    Guid Id,
    string Key,
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? DueDate,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? Color,
    string? Icon,
    OwnerDto Owner,
    IEnumerable<MemberDto> Members,
    TaskStatsDto TaskStats,
    decimal Progress,
    IReadOnlyList<ProjectTagDto> Tags,
    string? Category,
    string? Department,
    Guid DepartmentId,
    bool IsArchived,
    bool IsFavorite,
    PermissionsDto Permissions,
    IReadOnlyList<FileAttachmentDto>? ProjectAttachments,
    IDictionary<string, object>? CustomFields
);