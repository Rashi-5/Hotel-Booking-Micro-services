using Microsoft.EntityFrameworkCore;
using ChatbotService.Models;

namespace ChatbotService.Data
{
    public class ChatbotDbContext : DbContext
    {
        public ChatbotDbContext(DbContextOptions<ChatbotDbContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ChatMessage entity
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserMessage).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.BotResponse).HasMaxLength(2000);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SessionId);
            });
        }
    }
}
