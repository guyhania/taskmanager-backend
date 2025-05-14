namespace TaskReminderService.Configuration;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public string QueueName { get; set; } = "task-reminders";
}
