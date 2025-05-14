using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Options;
using TaskReminderService.Configuration;

namespace TaskReminderService.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqService(ILogger<RabbitMqService> logger, IOptions<RabbitMqSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public void Initialize()
    {
        var factory = new ConnectionFactory { HostName = _settings.HostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish(string queue, string messageJson)
    {
        _channel!.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        var body = Encoding.UTF8.GetBytes(messageJson);
        _channel.BasicPublish("", queue, null, body);
    }

    public void Subscribe(string queue, EventHandler<BasicDeliverEventArgs> onMessageReceived)
    {
        _channel!.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += onMessageReceived;
        _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
    }

    public void Acknowledge(ulong deliveryTag) =>
        _channel?.BasicAck(deliveryTag, multiple: false);

    public void Reject(ulong deliveryTag, bool requeue = true) =>
        _channel?.BasicNack(deliveryTag, multiple: false, requeue: requeue);

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
