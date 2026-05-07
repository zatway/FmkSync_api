using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Attachments;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskComments.Commands.UploadTaskCommentAttachments;

public class UploadTaskCommentAttachmentsHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser,
    IFileStorage storage
) : IRequestHandler<UploadTaskCommentAttachmentsCommand, IReadOnlyList<CommentAttachmentDto>>
{
    public async Task<IReadOnlyList<CommentAttachmentDto>> Handle(
        UploadTaskCommentAttachmentsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (!TaskAccessRules.UserCanAddComments(currentUser.Role))
            throw new ForbiddenException("Читатель не может прикреплять файлы к комментариям");

        var comment = await context.TaskComments
            .Include(c => c.Attachments)
            .Include(c => c.Task)
            .ThenInclude(t => t.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null) throw new NotFoundException("Комментарий не найден");
        if (!ProjectAccessRules.UserCanViewProject(
                currentUser.Role, userId, comment.Task.Project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к задаче");

        var created = new List<CommentAttachmentDto>();

        foreach (var file in request.Files)
        {
            if (file.Length <= 0) continue;

            await using var stream = file.OpenReadStream();
            var storedPath = await storage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

            var entity = new TaskCommentAttachment
            {
                Id = Guid.NewGuid(),
                TaskCommentId = comment.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                StoredPath = storedPath,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.TaskCommentAttachments.Add(entity);
            comment.Attachments.Add(entity);

            created.Add(new CommentAttachmentDto(
                FileIdCodec.TaskCommentAttachment(entity.Id),
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
                entity.CreatedAt
            ));
        }

        await context.SaveChangesAsync(cancellationToken);
        return created;
    }
}

