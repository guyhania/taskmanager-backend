using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskReminderService.Data;
using TaskReminderService.Models;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _services;
    private IConnection? _connection; // Make these nullable
    private IModel? _channel;     // Make these nullable

    public Worker(ILogger<Worker> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConnectToRabbitMQ(); // Call ConnectToRabbitMQ() once

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var overdueTasks = db.Tasks
                .Where(t => !t.IsReminderSent && t.DueDate < DateTime.Now)
                .ToList();

            foreach (var task in overdueTasks)
            {
                // 1. Send to queue
                if (_channel != null) // Check if _channel is not null
                {
                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(task));
                    _channel.BasicPublish(exchange: "", routingKey: "task-reminders", body: body);
                    _logger.LogInformation("Queued task: {Title}", task.Title);
                }
                else
                {
                    _logger.LogError("Channel is null. Cannot publish message.");
                }

                // 2. Mark as sent
                task.IsReminderSent = true;
            }

            await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private void ConnectToRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "task-reminders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var task = JsonSerializer.Deserialize<TaskItem>(json);

                _logger.LogWarning("Hi your Task is due: {Title}", task?.Title);
            };

            _channel.BasicConsume(queue: "task-reminders", autoAck: true, consumer: consumer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to RabbitMQ");
            // Consider adding retry logic here.  For example:
            // Task.Delay(5000).Wait(); // Wait 5 seconds before retry
            // ConnectToRabbitMQ();
        }
    }

    public override void Dispose()
    {
        if (_channel != null)  // Check for null before closing
        {
            _channel.Close();
        }
        if (_connection != null) // Check for null before closing
        {
            _connection.Close();
        }
        base.Dispose();
    }
}
