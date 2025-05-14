using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using RabbitMQ.Client.Events;
using TaskReminderService.Configuration;
using TaskReminderService.Data;
using TaskReminderService.Models;
using TaskReminderService.Services;
using Microsoft.EntityFrameworkCore;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _services;
    private readonly IRabbitMqService _mq;
    private readonly RabbitMqSettings _settings;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider services,
        IRabbitMqService mq,
        IOptions<RabbitMqSettings> options)
    {
        _logger = logger;
        _services = services;
        _mq = mq;
        _settings = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _mq.Initialize();
        _mq.Subscribe(_settings.QueueName, OnMessageReceived);
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
                var json = JsonSerializer.Serialize(task);
                _mq.Publish(_settings.QueueName, json);

                task.IsReminderSent = true;
                _logger.LogInformation("Queued task: {Title}", task.Title);
            }

            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private void OnMessageReceived(object? sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var json = System.Text.Encoding.UTF8.GetString(body);
            var task = JsonSerializer.Deserialize<TaskItem>(json);

            _logger.LogWarning("Hi your Task is due: {Title}", task?.Title);
            _mq.Acknowledge(ea.DeliveryTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process task message.");
            _mq.Reject(ea.DeliveryTag);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker stopping...");
        _mq.Dispose();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _mq.Dispose();
        base.Dispose();
    }
}
