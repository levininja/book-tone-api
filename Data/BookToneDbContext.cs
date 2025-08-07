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
        public DbSet<BatchProcessingLog> BatchProcessingLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookToneRecommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.BookId).IsRequired();
                entity.Property(e => e.Feedback).IsRequired();
                entity.Property(e => e.Tone).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<BatchProcessingLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.BookId).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ErrorType).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ErrorMessage).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
} 