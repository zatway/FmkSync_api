using Application.Common;
using Application.DTO.Knowledge;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Knowledge.Queries.GetKnowledgeArticleBySlug;

public class GetKnowledgeArticleBySlugHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetKnowledgeArticleBySlugQuery, KnowledgeArticleDetailDto?>
{
    public async Task<KnowledgeArticleDetailDto?> Handle(
        GetKnowledgeArticleBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim();
        if (string.IsNullOrEmpty(slug)) return null;

        var a = await context.KnowledgeArticles
            .AsNoTracking()
            .Include(x => x.Author)
            .Include(x => x.Project)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (a == null && Guid.TryParse(slug, out var articleId))
        {
            a = await context.KnowledgeArticles
                .AsNoTracking()
                .Include(x => x.Author)
                .Include(x => x.Project)
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == articleId, cancellationToken);
        }

        if (a == null) return null;

        await KnowledgeLinkValidation.EnsureArticleVisibleAsync(context, currentUser, a, cancellationToken);

        return KnowledgeArticleDtoFactory.ToDetailDto(a);
    }
}
