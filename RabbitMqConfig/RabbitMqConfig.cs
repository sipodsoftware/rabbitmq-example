using System.Text;
using RabbitMQ.Client;

namespace RabbitMq
{
    public class RabbitMqConfig
    {
        private static RabbitMqConfig? instance = null;

        private readonly IConnection connection;
        private readonly string connectionString = "amqp://guest:guest@localhost:5672/";

        private RabbitMqConfig()
        {
            this.connection = this.ConnectToRabbitMqServer();
        }

        public static RabbitMqConfig GetRabbitMqConfig
        {
            get
            {
                instance ??= new RabbitMqConfig();

                return instance;
            }
        }

        public void ConfigureQueueIfNotExist(
            string queue,
            string exchange,
            string routingKey,
            string deadLetterQueue,
            string deadLetterExchange,
            string deadLetterRoutingKey)
        {
            using var channel = this.connection.CreateModel();

            channel.ExchangeDeclare(deadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false);
            channel.QueueDeclare(deadLetterQueue, durable: true, exclusive: false, autoDelete: false, null);
            channel.QueueBind(deadLetterQueue, deadLetterExchange, deadLetterRoutingKey);

            channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true, autoDelete: false);
            channel.QueueDeclare(
                queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", deadLetterExchange },
                    { "x-dead-letter-routing-key", deadLetterRoutingKey}
                });
            channel.QueueBind(queue, exchange, routingKey, null);
        }

        public IConnection GetConnection()
        {
            return this.connection;
        }

        public void SendMessageToQueue<TModel>(
                            TModel message,
            string queue,
            string exchange,
            string routingKey,
            string deadLetterQueue,
            string deadLetterExchange,
            string deadLetterRoutingKey)
        {
            this.ConfigureQueueIfNotExist(
                queue,
                exchange,
                routingKey,
                deadLetterQueue,
                deadLetterExchange,
                deadLetterRoutingKey);

            byte[] serializedMessage = SerializeMessageForQueue(message);

            using var channel = this.connection.CreateModel();

            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            channel.BasicPublish(
                exchange,
                routingKey,
                properties,
                serializedMessage);
        }

        private static byte[] SerializeMessageForQueue<TModel>(TModel message)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            return Encoding.UTF8.GetBytes(json);
        }

        private IConnection ConnectToRabbitMqServer()
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(this.connectionString),
                AutomaticRecoveryEnabled = true,
                DispatchConsumersAsync = true
            };

            return connectionFactory.CreateConnection();
        }
    }
}