using Application.DTO.Knowledge;
using MediatR;

namespace Application.Knowledge.Commands.CreateKnowledgeArticle;

public record CreateKnowledgeArticleCommand(
    string Title,
    string? Slug,
    string ContentMarkdown,
    Guid? ParentId,
    int? SortOrder,
    Guid? ProjectId
) : IRequest<KnowledgeArticleDetailDto>;
