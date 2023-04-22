using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

namespace StockService.Bus
{
    public class EventBus
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public EventBus(RabbitMqConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration.HostName,
                Port = configuration.Port,
                UserName = configuration.Username,
                Password = configuration.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish<T>(T message, string exchange, string routingKey)
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Topic);
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish(exchange, routingKey, null, body);
        }

        public void Subscribe<T>(string exchange, string queueName, string routingKey, Func<T, Task> onMessageReceived)
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Topic);
            _channel.QueueDeclare(queueName, false, false, false, null);
            _channel.QueueBind(queueName, exchange, routingKey, null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("SUSCRIBE TO: " + routingKey);
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body));
                Console.WriteLine(message);
                await onMessageReceived(message);
            };
            _channel.BasicConsume(queueName, true, consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
