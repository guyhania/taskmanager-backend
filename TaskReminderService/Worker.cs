using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskReminderService.Data;
using TaskReminderService.Models;
using Microsoft.EntityFrameworkCore;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _services;

    private IConnection? _connection;
    private IModel? _channel;

    public Worker(ILogger<Worker> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "task-reminders", durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;

        _channel.BasicConsume(queue: "task-reminders", autoAck: false, consumer: consumer);

        _logger.LogInformation("Worker started and subscribed to task-reminders queue.");
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var overdueTasks = await db.Tasks
                .Where(t => !t.IsReminderSent && t.DueDate < DateTime.Now)
                .ToListAsync(stoppingToken);

            foreach (var task in overdueTasks)
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(task));
                _channel?.BasicPublish(
                    exchange: "",
                    routingKey: "task-reminders",
                    basicProperties: null,
                    body: body
                );

                task.IsReminderSent = true;
                _logger.LogInformation("Queued task: {Title}", task.Title);
            }

            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private void OnMessageReceived(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var task = JsonSerializer.Deserialize<TaskItem>(json);

            _logger.LogWarning("Hi your Task is due: {Title}", task?.Title);

            _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process task message.");
            _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        _logger.LogInformation("Worker stopped and RabbitMQ connection closed.");
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
