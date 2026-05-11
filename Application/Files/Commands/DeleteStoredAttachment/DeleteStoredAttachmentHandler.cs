using Application.Common;
using Application.Common.Exceptions;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Files.Commands.DeleteStoredAttachment;

public class DeleteStoredAttachmentHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser,
    IFileStorage fileStorage)
    : IRequestHandler<DeleteStoredAttachmentCommand>
{
    public async Task Handle(DeleteStoredAttachmentCommand request, CancellationToken cancellationToken)
    {
        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role ?? throw new UnauthorizedAccessException();

        if (!FileIdCodec.TryParse(request.FileId, out var prefix, out var id))
            throw new BadRequestException("Некорректный идентификатор файла.");

        switch (prefix)
        {
            case "av":
                throw new ForbiddenException("Удаление аватара этим способом недоступно.");
            case "ta":
                await DeleteTaskAttachmentAsync(id, uid, role, cancellationToken);
                return;
            case "pa":
                await DeleteProjectAttachmentAsync(id, uid, role, cancellationToken);
                return;
            case "tc":
                await DeleteTaskCommentAttachmentAsync(id, uid, role, cancellationToken);
                return;
            case "pc":
                await DeleteProjectCommentAttachmentAsync(id, uid, role, cancellationToken);
                return;
            case "ka":
                await DeleteKnowledgeArticleAttachmentAsync(id, cancellationToken);
                return;
            default:
                throw new BadRequestException("Неизвестный тип вложения.");
        }
    }

    private async Task DeleteTaskAttachmentAsync(Guid id, Guid uid, UserRole role, CancellationToken ct)
    {
        var att = await context.TaskAttachments
            .Include(a => a.ProjectTask)
            .ThenInclude(t => t.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Вложение не найдено");

        if (!ProjectAccessRules.UserCanViewProject(role, uid, att.ProjectTask.Project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к задаче");
        if (!TaskAccessRules.UserCanModifyTask(role, uid, att.ProjectTask))
            throw new ForbiddenException("Недостаточно прав для удаления вложения задачи");

        context.TaskAttachments.Remove(att);
        await context.SaveChangesAsync(ct);
        await fileStorage.TryDeleteAsync(att.StoredPath, ct);
    }

    private async Task DeleteProjectAttachmentAsync(Guid id, Guid uid, UserRole role, CancellationToken ct)
    {
        var att = await context.ProjectAttachments
            .Include(a => a.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Вложение не найдено");

        if (!ProjectAccessRules.UserCanViewProject(role, uid, att.Project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");
        if (!ProjectAccessRules.UserCanManageProjectsAndColumns(role))
            throw new ForbiddenException("Удалять файлы проекта могут только администратор или менеджер");

        context.ProjectAttachments.Remove(att);
        await context.SaveChangesAsync(ct);
        await fileStorage.TryDeleteAsync(att.StoredPath, ct);
    }

    private async Task DeleteTaskCommentAttachmentAsync(Guid id, Guid uid, UserRole role, CancellationToken ct)
    {
        var att = await context.TaskCommentAttachments
            .Include(a => a.TaskComment)
            .ThenInclude(c => c.Task)
            .ThenInclude(t => t.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Вложение не найдено");

        var comment = att.TaskComment;
        if (!ProjectAccessRules.UserCanViewProject(role, uid, comment.Task.Project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к задаче");

        var canDelete = comment.UserId == uid || role is UserRole.Admin or UserRole.Manager;
        if (!canDelete)
            throw new ForbiddenException("Удалить это вложение может автор комментария или администратор.");

        context.TaskCommentAttachments.Remove(att);
        await context.SaveChangesAsync(ct);
        await fileStorage.TryDeleteAsync(att.StoredPath, ct);
    }

    private async Task DeleteProjectCommentAttachmentAsync(Guid id, Guid uid, UserRole role, CancellationToken ct)
    {
        var att = await context.ProjectCommentAttachments
            .Include(a => a.ProjectComment)
            .ThenInclude(c => c.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Вложение не найдено");

        var comment = att.ProjectComment;
        if (!ProjectAccessRules.UserCanViewProject(role, uid, comment.Project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");

        var canDelete = comment.AuthorId == uid || role is UserRole.Admin or UserRole.Manager;
        if (!canDelete)
            throw new ForbiddenException("Удалить это вложение может автор комментария или администратор.");

        context.ProjectCommentAttachments.Remove(att);
        await context.SaveChangesAsync(ct);
        await fileStorage.TryDeleteAsync(att.StoredPath, ct);
    }

    private async Task DeleteKnowledgeArticleAttachmentAsync(Guid id, CancellationToken ct)
    {
        if (currentUser.Role is not UserRole.Admin and not UserRole.Manager)
            throw new ForbiddenException("Удалять вложения статей могут только администратор или менеджер.");

        var att = await context.KnowledgeArticleAttachments
            .Include(a => a.Article)
            .ThenInclude(ar => ar.Project)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Вложение не найдено");

        await KnowledgeLinkValidation.EnsureArticleVisibleAsync(context, currentUser, att.Article, ct);

        context.KnowledgeArticleAttachments.Remove(att);
        await context.SaveChangesAsync(ct);
        await fileStorage.TryDeleteAsync(att.StoredPath, ct);
    }
}
