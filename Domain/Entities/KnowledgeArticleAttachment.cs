using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class KnowledgeArticleAttachment
{
    public Guid Id { get; set; }

    public Guid KnowledgeArticleId { get; set; }

    [ForeignKey(nameof(KnowledgeArticleId))]
    public KnowledgeArticle Article { get; set; } = null!;

    [Required, MaxLength(260)]
    public string FileName { get; set; } = null!;

    [MaxLength(120)]
    public string? ContentType { get; set; }

    public long SizeBytes { get; set; }

    [Required, MaxLength(500)]
    public string StoredPath { get; set; } = null!;

    public Guid UploadedByUserId { get; set; }

    [ForeignKey(nameof(UploadedByUserId))]
    public User UploadedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
