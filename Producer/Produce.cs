using RabbitMq;

namespace Producer
{
    public class Produce
    {
        private static void Main()
        {
            RabbitMqConfig rabbitMq = RabbitMqConfig.GetRabbitMqConfig;

            while (true)
            {
                Console.WriteLine("Enter the message you want to send:");

                string? message = Console.ReadLine();

                rabbitMq.SendMessageToQueue(
                    message,
                    "testQueue",
                    "testExchange",
                    "testRoutingKey",
                    "testDeadLetterQueue",
                    "testDeadLetterExchange",
                    "testDeadLetterRoutingKey");
            }
        }
    }
}