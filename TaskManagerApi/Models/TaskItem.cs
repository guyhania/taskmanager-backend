using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.Models;

public class TaskItem
{
    public int Id { get; set; }

    // Task fields
    public required string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Priority { get; set; } = "Medium"; // Low, Medium, High

    // Embedded user info
    public string FullName { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    [EmailAddress]

    public required string Email { get; set; } = string.Empty;

    // For RabbitMQ later
    public bool IsReminderSent { get; set; } = false;
}
