using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Persistence.Infrastructure.Configurations;

public class TaskAttachmentConfiguration : IEntityTypeConfiguration<TaskAttachment>
{
    public void Configure(EntityTypeBuilder<TaskAttachment> entity)
    {
        entity.HasKey(a => a.Id);

        entity.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(255);

        entity.Property(a => a.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);

        entity.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(100);

        entity.HasOne(a => a.TaskItem)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}