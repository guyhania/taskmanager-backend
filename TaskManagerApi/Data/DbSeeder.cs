using TaskManagerApi.Models;

namespace TaskManagerApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Tasks.Any()) return;

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Overdue: Fix login bug",
                Description = "Urgent fix needed for user login failure",
                DueDate = DateTime.Now.AddDays(-2),
                Priority = "High",
                FullName = "Guy Hania",
                Telephone = "+972501234567",
                Email = "guy.hania@example.com",
                IsReminderSent = false
            },
            new TaskItem
            {
                Title = "Overdue: Submit interview app",
                Description = "Make sure README and demo are ready",
                DueDate = DateTime.Now.AddHours(-12),
                Priority = "High",
                FullName = "Guy Hania",
                Telephone = "+972501234567",
                Email = "guy.hania@example.com",
                IsReminderSent = false
            },
            new TaskItem
            {
                Title = "Future: Deploy frontend",
                Description = "Set up hosting and CDN config",
                DueDate = DateTime.Now.AddDays(2),
                Priority = "Medium",
                FullName = "Guy Hania",
                Telephone = "+972501234567",
                Email = "guy.hania@example.com",
                IsReminderSent = false
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}
