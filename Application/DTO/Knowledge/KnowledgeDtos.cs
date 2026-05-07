using Application.Common;
using Application.DTO.Attachments;
using Domain.Entities;

namespace Application.DTO.Knowledge;

public record KnowledgeArticleListItemDto(
    Guid Id,
    string Title,
    string Slug,
    Guid? ParentId,
    int SortOrder,
    DateTimeOffset? UpdatedAt,
    Guid? ProjectId,
    string? ProjectKey,
    string? ProjectName,
    Guid? ProjectTaskId,
    /// <summary>Краткий ключ задачи, напр. CRM-12.</summary>
    string? TaskDisplayKey);

public record KnowledgeArticleDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string ContentMarkdown,
    Guid? ParentId,
    Guid AuthorId,
    string AuthorName,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid? ProjectId,
    string? ProjectKey,
    string? ProjectName,
    Guid? ProjectTaskId,
    string? TaskDisplayKey,
    string? TaskTitle,
    IReadOnlyList<CommentAttachmentDto> Attachments);

public static class KnowledgeArticleDtoFactory
{
    public static KnowledgeArticleDetailDto ToDetailDto(KnowledgeArticle a)
    {
        var attachments = (a.Attachments ?? Array.Empty<KnowledgeArticleAttachment>())
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CommentAttachmentDto(
                FileIdCodec.KnowledgeArticleAttachment(x.Id),
                x.FileName,
                x.ContentType,
                x.SizeBytes,
                x.CreatedAt))
            .ToList();

        return new KnowledgeArticleDetailDto(
            a.Id,
            a.Title,
            a.Slug,
            a.ContentMarkdown,
            a.ParentId,
            a.AuthorId,
            a.Author.FullName,
            a.SortOrder,
            a.CreatedAt,
            a.UpdatedAt,
            a.ProjectId,
            a.Project?.Key,
            a.Project?.Name,
            null,
            null,
            null,
            attachments);
    }
}
