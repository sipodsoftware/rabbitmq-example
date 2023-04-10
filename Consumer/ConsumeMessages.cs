using System.Text;
using RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer
{
    public class ConsumeMessages
    {
        private readonly IModel channel;
        private readonly RabbitMqConfig rabbitMqConfig;

        public ConsumeMessages()
        {
            this.rabbitMqConfig = RabbitMqConfig.GetRabbitMqConfig;

            this.channel = this.rabbitMqConfig.GetConnection().CreateModel();

            this.Subscribe();
        }

        public void Subscribe()
        {
            this.rabbitMqConfig.ConfigureQueueIfNotExist(
                "testQueue",
                "testExchange",
                "testRoutingKey",
                "testDeadLetterQueue",
                "testDeadLetterExchange",
                "testDeadLetterRoutingKey");

            this.channel.BasicQos(0, 1, false);

            AsyncEventingBasicConsumer eventingBasicConsumer = new(this.channel);

            eventingBasicConsumer.Received += ReceivedMessage;

            channel.BasicConsume("testQueue", false, eventingBasicConsumer);

            Console.WriteLine("Listening queue...");

            Console.ReadKey();
        }

        private static string DeserializeMessageFromQueue(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private Task ReceivedMessage(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                Console.WriteLine("Message received: " + DeserializeMessageFromQueue(e.Body.ToArray()));
                channel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception)
            {
                channel.BasicNack(e.DeliveryTag, false, false);
            }

            return Task.CompletedTask;
        }
    }
}