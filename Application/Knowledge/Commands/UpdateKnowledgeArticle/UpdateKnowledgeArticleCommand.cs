using Application.DTO.Knowledge;
using MediatR;

namespace Application.Knowledge.Commands.UpdateKnowledgeArticle;

public record UpdateKnowledgeArticleCommand(
    Guid Id,
    string? Title,
    string? Slug,
    string? ContentMarkdown,
    Guid? ParentId,
    int? SortOrder,
    Guid? ProjectId,
    bool ScopeChanged = false,
    bool ParentChanged = false
) : IRequest<KnowledgeArticleDetailDto?>;
