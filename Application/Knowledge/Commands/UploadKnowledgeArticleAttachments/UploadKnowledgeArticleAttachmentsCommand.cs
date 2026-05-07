using Application.DTO.Attachments;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Knowledge.Commands.UploadKnowledgeArticleAttachments;

public record UploadKnowledgeArticleAttachmentsCommand(Guid ArticleId, IReadOnlyList<IFormFile> Files)
    : IRequest<IReadOnlyList<CommentAttachmentDto>>;
