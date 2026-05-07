using Application.Common.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Common;

public static class KnowledgeLinkValidation
{
    /// <summary>Проверяет доступ к проекту и возвращает валидный projectId (или null для общей статьи).</summary>
    public static async Task<Guid?> ValidateProjectScopeAsync(
        IKomSyncContext context,
        ICurrentUserService currentUser,
        Guid? requestedProjectId,
        CancellationToken cancellationToken)
    {
        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role;

        if (requestedProjectId.HasValue)
        {
            var project = await context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == requestedProjectId.Value, cancellationToken)
                ?? throw new NotFoundException("Проект не найден");

            if (!ProjectAccessRules.UserCanViewProject(role, uid, project, currentUser.DepartmentId))
                throw new ForbiddenException("Нет доступа к проекту");

            return requestedProjectId;
        }

        return null;
    }

    public static async Task ValidateParentScopeAsync(
        IKomSyncContext context,
        Guid? parentId,
        Guid? projectId,
        CancellationToken cancellationToken)
    {
        if (!parentId.HasValue) return;

        var parent = await context.KnowledgeArticles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == parentId.Value, cancellationToken)
            ?? throw new BadRequestException("Родительская статья не найдена");

        if (parent.ProjectId != projectId)
            throw new BadRequestException("Вложенная статья должна быть в том же проекте, что и родитель.");
    }

    public static async Task EnsureArticleVisibleAsync(
        IKomSyncContext context,
        ICurrentUserService currentUser,
        KnowledgeArticle article,
        CancellationToken cancellationToken)
    {
        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role;

        if (ProjectAccessRules.CanViewAllProjects(role))
            return;

        if (article.ProjectId == null)
            return;

        if (article.ProjectId.HasValue)
        {
            var p = await context.Projects
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == article.ProjectId.Value, cancellationToken);
            if (p != null && ProjectAccessRules.UserCanViewProject(role, uid, p, currentUser.DepartmentId))
                return;
        }

        throw new ForbiddenException("Нет доступа к этой статье");
    }
}
