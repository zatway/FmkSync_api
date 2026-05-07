using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Attachments;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tasks.Commands.UploadTaskAttachment;

public class UploadTaskAttachmentHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser,
    IFileStorage storage)
    : IRequestHandler<UploadTaskAttachmentCommand, IReadOnlyList<FileAttachmentDto>>
{
    public async Task<IReadOnlyList<FileAttachmentDto>> Handle(
        UploadTaskAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role;

        var task = await context.Tasks
            .Include(t => t.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
            throw new NotFoundException("Задача не найдена");

        if (!ProjectAccessRules.UserCanViewProject(role, userId, task.Project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к задаче");
        if (!TaskAccessRules.UserCanModifyTask(role, userId, task))
            throw new ForbiddenException("Недостаточно прав для вложений к задаче");

        var created = new List<FileAttachmentDto>();

        foreach (var file in request.Files)
        {
            if (file.Length <= 0) continue;

            await using var stream = file.OpenReadStream();
            var storedPath = await storage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

            var entity = new TaskAttachment
            {
                Id = Guid.NewGuid(),
                ProjectTaskId = task.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                StoredPath = storedPath,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.TaskAttachments.Add(entity);

            created.Add(new FileAttachmentDto(
                FileIdCodec.TaskAttachment(entity.Id),
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
                entity.CreatedAt));
        }

        await context.SaveChangesAsync(cancellationToken);
        return created;
    }
}
