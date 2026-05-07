using Application.DTO.Attachments;
using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects.Commands.UploadProjectCommentAttachments;

public class UploadProjectCommentAttachmentsHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser,
    IFileStorage storage,
    IRealtimeNotificationPublisher notifications
) : IRequestHandler<UploadProjectCommentAttachmentsCommand, IReadOnlyList<CommentAttachmentDto>>
{
    public async Task<IReadOnlyList<CommentAttachmentDto>> Handle(
        UploadProjectCommentAttachmentsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var comment = await context.ProjectComments
            .Include(c => c.Attachments)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null) throw new KeyNotFoundException("Comment not found");

        var created = new List<CommentAttachmentDto>();

        foreach (var file in request.Files)
        {
            if (file.Length <= 0) continue;
            await using var stream = file.OpenReadStream();
            var storedPath = await storage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

            var entity = new ProjectCommentAttachment
            {
                Id = Guid.NewGuid(),
                ProjectCommentId = comment.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                StoredPath = storedPath,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.ProjectCommentAttachments.Add(entity);
            comment.Attachments.Add(entity);

            created.Add(new CommentAttachmentDto(
                FileIdCodec.ProjectCommentAttachment(entity.Id),
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
                entity.CreatedAt
            ));
        }

        await context.SaveChangesAsync(cancellationToken);

        // notify project comment thread participants (basic: author of root + mentions later)
        if (comment.AuthorId != userId)
        {
            await notifications.PublishToUserAsync(
                comment.AuthorId,
                "project.comment.attachment.added",
                new { projectId = comment.ProjectId, commentId = comment.Id, byUserId = userId },
                cancellationToken);
        }

        return created;
    }
}

