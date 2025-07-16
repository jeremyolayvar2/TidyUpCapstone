using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.Entities;

namespace TidyUpCapstone.Data  
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ItemPost> ItemPosts { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<ItemCondition> ItemConditions { get; set; }
        public DbSet<ItemLocation> ItemLocations { get; set; }
        public DbSet<Messages> Message { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItemCategory>().HasData(
                new ItemCategory { Id = 1, Name = "Books & Stationery" },
                new ItemCategory { Id = 2, Name = "Electronics & Gadgets" },
                new ItemCategory { Id = 3, Name = "Toys & Games" },
                new ItemCategory { Id = 4, Name = "Home & Kitchen" },
                new ItemCategory { Id = 5, Name = "Furniture" },
                new ItemCategory { Id = 6, Name = "Appliances" },
                new ItemCategory { Id = 7, Name = "Health & Beauty" },
                new ItemCategory { Id = 8, Name = "Crafts & DIY Supplies" },
                new ItemCategory { Id = 9, Name = "School & Office Supplies" }
            );

            modelBuilder.Entity<ItemCondition>().HasData(
                new ItemCondition { Id = 1, Name = "Brand New" },
                new ItemCondition { Id = 2, Name = "Like New" },
                new ItemCondition { Id = 3, Name = "Gently Used" },
                new ItemCondition { Id = 4, Name = "Visible Wear" },
                new ItemCondition { Id = 5, Name = "For Repair/Parts" }
            );

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.TokenBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Messages>().ToTable("Message");

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Buyer)
                .WithMany()
                .HasForeignKey(m => m.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Seller)
                .WithMany()
                .HasForeignKey(m => m.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.ItemPost)
                .WithMany()
                .HasForeignKey(m => m.ItemPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}