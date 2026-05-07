namespace Application.DTO.Projects;

public record ChangedByDto(
    Guid Id,
    string Name,
    bool HasAvatar
);