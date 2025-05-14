using Microsoft.EntityFrameworkCore;
using TaskReminderService.Models;

namespace TaskReminderService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}
