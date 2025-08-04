using Microsoft.EntityFrameworkCore;
using BookToneApi.Models;

namespace BookToneApi.Data
{
    public class BookToneDbContext : DbContext
    {
        public BookToneDbContext(DbContextOptions<BookToneDbContext> options)
            : base(options)
        {
        }

        public DbSet<BookToneRecommendation> BookToneRecommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookToneRecommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.BookTitle).IsRequired().HasMaxLength(500);
                entity.Property(e => e.BookAuthor).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Feedback).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
} 