using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TidyUpCapstone.Models.Entities.Authentication;


namespace TidyUpCapstone.Models.Configurations.EntityConfigurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("app_user");

            builder.HasKey(e => e.UserId);

            // Configure properties
            builder.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.TokenBalance)
                .HasPrecision(10, 2);

            // Configure indexes
            builder.HasIndex(e => e.UserName).IsUnique();
            builder.HasIndex(e => e.Email).IsUnique();
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.Role);
            builder.HasIndex(e => new { e.IsVerified, e.Status });
            builder.HasIndex(e => new { e.ExternalProvider, e.ExternalUserId });
            builder.HasIndex(e => e.RegistrationMethod);
            builder.HasIndex(e => e.EmailConfirmed);
            builder.HasIndex(e => new { e.LockoutEnabled, e.LockoutEnd });

            // Configure enum properties as strings
            builder.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(e => e.RegistrationMethod)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Configure relationships
            builder.HasOne(e => e.ManagedByAdmin)
                .WithMany(e => e.ManagedUsers)
                .HasForeignKey(e => e.ManagedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure check constraints
            builder.HasCheckConstraint("chk_positive_token_balance", "token_balance >= 0");

            builder.HasCheckConstraint("chk_sso_user_validation",
                "(external_provider IS NULL AND password_hash IS NOT NULL) OR " +
                "(external_provider IS NOT NULL AND external_user_id IS NOT NULL)");
        }
    }
}