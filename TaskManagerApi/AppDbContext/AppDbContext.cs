using Microsoft.EntityFrameworkCore;

using TaskManagerApi.Models;

namespace TaskManagerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /* Add index on IsReminderSent + DueDate */
        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => new { t.IsReminderSent, t.DueDate })
            .HasDatabaseName("IX_Tasks_ReminderQuery");

        base.OnModelCreating(modelBuilder);
    }
}
