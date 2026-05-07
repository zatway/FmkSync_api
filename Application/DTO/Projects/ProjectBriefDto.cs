namespace Application.DTO.Projects;

public record ProjectBriefDto(
    Guid Id,
    string Key,
    string Name,
    string? Description,
    Guid OwnerId,
    string OwnerName,
    bool OwnerHasAvatar,
    int MemberCount,
    int TaskCount,
    int OpenTaskCount,
    int? CompletedTaskCount,
    decimal Progress,
    DateTimeOffset? DueDate,
    DateTimeOffset? LastActivityAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? Color,
    string? Icon,
    bool IsArchived
);