using MediatR;

namespace Application.Files.Commands.DeleteStoredAttachment;

/// <summary>Удаление вложения по закодированному идентификатору (префикс как в <see cref="Application.Common.FileIdCodec"/>).</summary>
public record DeleteStoredAttachmentCommand(string FileId) : IRequest;
