using Application.DTO.Knowledge;
using MediatR;

namespace Application.Knowledge.Queries.GetKnowledgeArticles;

public record GetKnowledgeArticlesQuery(Guid? ProjectId = null)
    : IRequest<IReadOnlyList<KnowledgeArticleListItemDto>>;
