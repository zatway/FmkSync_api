using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Attachments;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects.Commands.UploadProjectAttachment;

public class UploadProjectAttachmentHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser,
    IFileStorage storage)
    : IRequestHandler<UploadProjectAttachmentCommand, IReadOnlyList<FileAttachmentDto>>
{
    public async Task<IReadOnlyList<FileAttachmentDto>> Handle(
        UploadProjectAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role;

        var project = await context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException("Проект не найден");

        if (!ProjectAccessRules.UserCanViewProject(role, userId, project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");
        if (!ProjectAccessRules.UserCanManageProjectsAndColumns(role))
            throw new ForbiddenException("Прикреплять файлы к проекту могут только администратор или менеджер");

        var created = new List<FileAttachmentDto>();

        foreach (var file in request.Files)
        {
            if (file.Length <= 0) continue;

            await using var stream = file.OpenReadStream();
            var storedPath = await storage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

            var entity = new ProjectAttachment
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                StoredPath = storedPath,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.ProjectAttachments.Add(entity);

            created.Add(new FileAttachmentDto(
                FileIdCodec.ProjectAttachment(entity.Id),
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
                entity.CreatedAt));
        }

        await context.SaveChangesAsync(cancellationToken);
        return created;
    }
}
