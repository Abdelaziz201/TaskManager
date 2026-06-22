using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Applab.Domain.Entities;


namespace TaskManager.Applab.Persistence.Infrastructure.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> entity)
    {
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Title)
               .IsRequired()
               .HasMaxLength(150);

        entity.Property(t => t.Description)
                .HasMaxLength(500);

        entity.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

        entity.Property(t => t.DueDate)
                .IsRequired();

        entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
    }
}
