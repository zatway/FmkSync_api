using Application.Common.Exceptions;
using Application.Common;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Commands.DeleteUserAdmin;

public class DeleteUserAdminHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<DeleteUserAdminCommand, bool>
{
    public async Task<bool> Handle(DeleteUserAdminCommand request, CancellationToken cancellationToken)
    {
        var adminId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        if (request.UserId == adminId)
            throw new BadRequestException("Нельзя удалить свою учётную запись.");

        var user = await context.Users
            .Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) return false;

        if (SystemAdminProtection.IsSystemAdminEmail(user.Email))
            throw new BadRequestException("Нельзя удалить системного администратора.");

        var snapshotName = user.FullName.Trim();

        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        // Владение проектами → текущий админ
        await context.Projects
            .Where(p => p.OwnerId == user.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.OwnerId, _ => adminId), cancellationToken);

        // Участие в проектах
        foreach (var p in user.Projects.ToList())
            user.Projects.Remove(p);

        // История: сохраняем ФИО, отвязываем пользователя
        await context.TaskHistories
            .Where(h => h.ChangedById == user.Id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(h => h.ChangedByDisplayName,
                        h => h.ChangedByDisplayName ?? snapshotName)
                    .SetProperty(h => h.ChangedById, _ => (Guid?)null),
                cancellationToken);

        await context.ProjectHistories
            .Where(h => h.ChangedById == user.Id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(h => h.ChangedByDisplayName,
                        h => h.ChangedByDisplayName ?? snapshotName)
                    .SetProperty(h => h.ChangedById, _ => (Guid?)null),
                cancellationToken);

        // Статьи БЗ — перенос автора
        await context.KnowledgeArticles
            .Where(a => a.AuthorId == user.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.AuthorId, _ => adminId), cancellationToken);

        // Комментарии к задачам
        await context.TaskCommentAttachments
            .Where(a => context.TaskComments.Any(c => c.Id == a.TaskCommentId && c.UserId == user.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await context.TaskComments
            .Where(c => c.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        // Комментарии к проектам (дерево)
        await RemoveProjectCommentsByAuthorAsync(context, user.Id, cancellationToken);

        await context.ProjectTaskWatchers
            .Where(w => w.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await context.RefreshTokens
            .Where(t => t.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await context.PasswordResetTokens
            .Where(t => t.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await context.ApplicationForRegistrations
            .Where(a => a.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return true;
    }

    private static async Task RemoveProjectCommentsByAuthorAsync(
        IKomSyncContext context,
        Guid authorId,
        CancellationToken cancellationToken)
    {
        while (await context.ProjectComments.AnyAsync(c => c.AuthorId == authorId, cancellationToken))
        {
            var leafId = await context.ProjectComments
                .Where(c => c.AuthorId == authorId)
                .Where(c => !context.ProjectComments.Any(ch => ch.ParentId == c.Id))
                .Select(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (leafId != Guid.Empty)
            {
                await context.ProjectCommentAttachments
                    .Where(a => a.ProjectCommentId == leafId)
                    .ExecuteDeleteAsync(cancellationToken);
                await context.ProjectComments.Where(c => c.Id == leafId).ExecuteDeleteAsync(cancellationToken);
                continue;
            }

            var cId = await context.ProjectComments
                .Where(c => c.AuthorId == authorId)
                .Select(c => new { c.Id, c.ParentId })
                .FirstAsync(cancellationToken);

            var parentId = cId.ParentId;
            await context.ProjectComments
                .Where(ch => ch.ParentId == cId.Id)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(ch => ch.ParentId, _ => parentId),
                    cancellationToken);

            await context.ProjectCommentAttachments
                .Where(a => a.ProjectCommentId == cId.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await context.ProjectComments.Where(c => c.Id == cId.Id).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
