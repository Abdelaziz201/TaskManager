using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Persistence.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);

        entity.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
    }
}