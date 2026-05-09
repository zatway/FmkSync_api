using Application.Common;
using Application.DTO.Knowledge;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Knowledge.Queries.GetKnowledgeArticles;

public class GetKnowledgeArticlesHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetKnowledgeArticlesQuery, IReadOnlyList<KnowledgeArticleListItemDto>>
{
    public async Task<IReadOnlyList<KnowledgeArticleListItemDto>> Handle(
        GetKnowledgeArticlesQuery request,
        CancellationToken cancellationToken)
    {
        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role;

        IQueryable<Domain.Entities.KnowledgeArticle> query = context.KnowledgeArticles.AsNoTracking();

        if (request.ProjectId.HasValue)
        {
            var project = await context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId.Value, cancellationToken)
                ?? throw new Application.Common.Exceptions.NotFoundException("Проект не найден");

            if (!ProjectAccessRules.UserCanViewProject(role, uid, project, currentUser.DepartmentId))
                throw new Application.Common.Exceptions.ForbiddenException("Нет доступа к проекту");

            var pid = request.ProjectId.Value;
            query = query.Where(a => a.ProjectId == pid);
        }
        else if (!ProjectAccessRules.CanViewAllProjects(role))
        {
            var accessibleIds = context.Projects
                .WhereUserCanSeeProject(role, uid, currentUser.DepartmentId)
                .Select(p => p.Id);

            query = query.Where(a =>
                a.ProjectId == null
                || (a.ProjectId != null && accessibleIds.Contains(a.ProjectId.Value)));
        }

        var rows = await query
            .Include(a => a.Project)
            .OrderBy(a => a.ParentId)
            .ThenBy(a => a.SortOrder)
            .ThenBy(a => a.Title)
            .ToListAsync(cancellationToken);

        return rows.Select(MapList).ToList();
    }

    private static KnowledgeArticleListItemDto MapList(Domain.Entities.KnowledgeArticle a)
    {
        return new KnowledgeArticleListItemDto(
            a.Id,
            a.Title,
            a.Slug,
            a.ParentId,
            a.SortOrder,
            a.UpdatedAt,
            a.ProjectId,
            a.Project?.Key,
            a.Project?.Name);
    }
}
