namespace Application.DTO.Attachments;

public record FileAttachmentDto(
    string Id,
    string FileName,
    string? ContentType,
    long SizeBytes,
    DateTime CreatedAt
);
