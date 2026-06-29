using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserApi.Domains;

namespace UserApi.Infrastructures.EntityFramework.EntityConfigurations;

public class UserTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(400);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}