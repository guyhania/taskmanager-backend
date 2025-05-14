using RabbitMQ.Client.Events;
using System;

namespace TaskReminderService.Services;

public interface IRabbitMqService : IDisposable
{
    void Publish(string queue, string messageJson);
    void Subscribe(string queue, EventHandler<BasicDeliverEventArgs> onMessageReceived);
    void Acknowledge(ulong deliveryTag);
    void Reject(ulong deliveryTag, bool requeue = true);
    void Initialize();
}
