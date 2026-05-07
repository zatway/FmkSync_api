using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Attachments;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Knowledge.Commands.UploadKnowledgeArticleAttachments;

public class UploadKnowledgeArticleAttachmentsHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser,
    IFileStorage storage)
    : IRequestHandler<UploadKnowledgeArticleAttachmentsCommand, IReadOnlyList<CommentAttachmentDto>>
{
    public async Task<IReadOnlyList<CommentAttachmentDto>> Handle(
        UploadKnowledgeArticleAttachmentsCommand request,
        CancellationToken cancellationToken)
    {
        var role = currentUser.Role;
        if (role is not UserRole.Admin and not UserRole.Manager)
            throw new ForbiddenException("Загрузка вложений доступна администраторам и руководителям");

        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var article = await context.KnowledgeArticles
            .Include(a => a.Project)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken)
            ?? throw new NotFoundException("Статья не найдена");

        await KnowledgeLinkValidation.EnsureArticleVisibleAsync(context, currentUser, article, cancellationToken);

        var created = new List<CommentAttachmentDto>();

        foreach (var file in request.Files)
        {
            if (file.Length <= 0) continue;

            await using var stream = file.OpenReadStream();
            var storedPath = await storage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

            var entity = new KnowledgeArticleAttachment
            {
                Id = Guid.NewGuid(),
                KnowledgeArticleId = article.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                StoredPath = storedPath,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.KnowledgeArticleAttachments.Add(entity);
            article.Attachments.Add(entity);

            created.Add(new CommentAttachmentDto(
                FileIdCodec.KnowledgeArticleAttachment(entity.Id),
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
                entity.CreatedAt));
        }

        await context.SaveChangesAsync(cancellationToken);
        return created;
    }
}
