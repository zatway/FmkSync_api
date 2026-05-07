using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class KnowledgeArticleAttachmentConfiguration : IEntityTypeConfiguration<KnowledgeArticleAttachment>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticleAttachment> builder)
    {
        builder.HasOne(x => x.Article)
            .WithMany(a => a.Attachments)
            .HasForeignKey(x => x.KnowledgeArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UploadedBy)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
