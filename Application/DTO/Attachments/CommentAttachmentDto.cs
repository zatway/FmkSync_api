namespace Application.DTO.Attachments;

public record CommentAttachmentDto(
    string Id,
    string FileName,
    string? ContentType,
    long SizeBytes,
    DateTime CreatedAt
);

