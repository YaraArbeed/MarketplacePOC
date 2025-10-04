using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MarketplacePOC.Core
{
    public class RabbitMqService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqService()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void DeclareQueueWithDLQ(string mainQueue, string deadLetterQueue)
        {
            // Dead-letter queue (quorum)
            _channel.QueueDeclare(deadLetterQueue, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object> { { "x-queue-type", "quorum" } });

            // Main queue (quorum) with DLX
            var args = new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" },
                { "x-dead-letter-exchange", "" }, // default exchange
                { "x-dead-letter-routing-key", deadLetterQueue }
            };

            _channel.QueueDeclare(mainQueue, durable: true, exclusive: false, autoDelete: false, arguments: args);
        }

        public void PublishOrder(string queueName, Order order)
        {
            var json = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            Console.WriteLine($"Published Order {order.OrderId} to queue {queueName}");
        }

        public void ConsumeOrders(string queueName, Action<Order> processOrder)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    var order = JsonSerializer.Deserialize<Order>(json);
                    if (order == null) throw new Exception("Deserialization failed");

                    processOrder(order);

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}, sending to DLQ");
                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            Console.WriteLine($"Started consuming queue '{queueName}'...");
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
