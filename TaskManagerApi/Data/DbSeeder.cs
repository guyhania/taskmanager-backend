using TaskManagerApi.Models;

namespace TaskManagerApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Tasks.Any()) return;

        var tasks = new List<TaskItem>();

        // 5 overdue tasks
        for (int i = 1; i <= 5; i++)
        {
            tasks.Add(new TaskItem
            {
                Title = $"Overdue Task {i}",
                Description = $"This task was due in the past",
                DueDate = DateTime.Now.AddDays(-i),
                Priority = "High",
                FullName = $"Overdue User {i}",
                Telephone = $"05000000{i:D2}", // Ensures 10 digits
                Email = $"overdue{i}@example.com",
                IsReminderSent = false
            });
        }

        // 5 due today
        for (int i = 1; i <= 5; i++)
        {
            tasks.Add(new TaskItem
            {
                Title = $"Today Task {i}",
                Description = $"This task is due today",
                DueDate = DateTime.Today.AddHours(9 + i), // Spaced across the day
                Priority = "Medium",
                FullName = $"Today User {i}",
                Telephone = $"05222222{i:D2}",
                Email = $"today{i}@example.com",
                IsReminderSent = false
            });
        }

        // 5 future tasks
        for (int i = 1; i <= 5; i++)
        {
            tasks.Add(new TaskItem
            {
                Title = $"Future Task {i}",
                Description = $"This task is due in the future",
                DueDate = DateTime.Now.AddDays(i),
                Priority = "Low",
                FullName = $"Future User {i}",
                Telephone = $"05333333{i:D2}",
                Email = $"future{i}@example.com",
                IsReminderSent = false
            });
        }

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}
