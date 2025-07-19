using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Configurations.EntityConfigurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("items");

            builder.HasKey(e => e.ItemId);

            // Configure properties
            builder.Property(e => e.ItemTitle)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);

            // Configure decimal precision
            builder.Property(e => e.AdjustedTokenPrice)
                .HasPrecision(10, 2);

            builder.Property(e => e.FinalTokenPrice)
                .HasPrecision(10, 2);

            builder.Property(e => e.AiSuggestedPrice)
                .HasPrecision(10, 2);

            builder.Property(e => e.Latitude)
                .HasPrecision(10, 8);

            builder.Property(e => e.Longitude)
                .HasPrecision(11, 8);

            builder.Property(e => e.AiConditionScore)
                .HasPrecision(5, 2);

            builder.Property(e => e.AiConfidenceLevel)
                .HasPrecision(5, 2);

            // Configure enum properties as strings
            builder.Property(e => e.AiProcessingStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Configure indexes for performance
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.CategoryId);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.LocationId);
            builder.HasIndex(e => e.DatePosted);
            builder.HasIndex(e => new { e.UserId, e.Status });
            builder.HasIndex(e => new { e.CategoryId, e.Status });
            builder.HasIndex(e => new { e.LocationId, e.Status });
            builder.HasIndex(e => new { e.FinalTokenPrice, e.Status });
            builder.HasIndex(e => new { e.Latitude, e.Longitude });

            // Configure relationships
            builder.HasOne(e => e.User)
                .WithMany(u => u.Items)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Condition)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Location)
                .WithMany(l => l.Items)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure check constraints
            builder.HasCheckConstraint("chk_positive_prices",
                "adjusted_token_price >= 0 AND final_token_price >= 0 AND " +
                "(ai_suggested_price IS NULL OR ai_suggested_price >= 0)");

            builder.HasCheckConstraint("chk_valid_coordinates",
                "(latitude IS NULL AND longitude IS NULL) OR " +
                "(latitude BETWEEN -90 AND 90 AND longitude BETWEEN -180 AND 180)");

            builder.HasCheckConstraint("chk_expiry_future",
                "expires_at IS NULL OR expires_at > date_posted");
        }
    }
}