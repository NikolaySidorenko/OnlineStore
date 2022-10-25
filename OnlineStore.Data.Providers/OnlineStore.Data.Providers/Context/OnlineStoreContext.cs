using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data.Providers.Context.Entities;
using OnlineStore.Data.Providers.Context.Entities.Identity;
using System.Collections.Generic;

namespace OnlineStore.Data.Providers.Context
{
    public class OnlineStoreContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public OnlineStoreContext(DbContextOptions options)
        : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Name = "Book 1" },
                new Book { Id = 2, Name = "Book 2" }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Cat 1" },
                new Category { Id = 2, Name = "Cat 2" }
            );

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Claims)
                .WithOne(c => c.User)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Categories)
                .WithMany(c => c.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BooksCategories",
                    r => r.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                    l => l.HasOne<Book>().WithMany().HasForeignKey("BookId"),
                    je =>
                    {
                        je.HasKey("BookId", "CategoryId");
                        je.HasData(
                            new { BookId = 1, CategoryId = 1 },
                            new { BookId = 1, CategoryId = 2 },
                            new { BookId = 2, CategoryId = 1 },
                            new { BookId = 2, CategoryId = 2 }
                        );
                    }
                );

            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = 1,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                },
                new ApplicationRole
                {
                    Id = 2,
                    Name = "Author",
                    NormalizedName = "AUTHOR",
                },
                new ApplicationRole
                {
                    Id = 3,
                    Name = "User",
                    NormalizedName = "USER",
                }
            );
        }
    }
}
