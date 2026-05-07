using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities;

/// <summary>Статья базы знаний (Markdown).</summary>
public class KnowledgeArticle : BaseEntity
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(320)]
    public string Slug { get; set; } = null!;

    public string ContentMarkdown { get; set; } = "";

    public Guid? ParentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    public KnowledgeArticle? Parent { get; set; }

    public ICollection<KnowledgeArticle> Children { get; set; } = new List<KnowledgeArticle>();

    /// <summary>Привязка к проекту (область знаний).</summary>
    public Guid? ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project? Project { get; set; }

    /// <summary>
    /// Legacy: task link is deprecated, knowledge articles are project-scoped only.
    /// Left for backward compatibility with existing data.
    /// </summary>
    public Guid? ProjectTaskId { get; set; }

    [ForeignKey(nameof(ProjectTaskId))]
    public ProjectTask? LinkedTask { get; set; }

    public Guid AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public User Author { get; set; } = null!;

    public int SortOrder { get; set; }

    public ICollection<KnowledgeArticleAttachment> Attachments { get; set; } = new List<KnowledgeArticleAttachment>();
}
