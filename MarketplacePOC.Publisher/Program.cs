using MarketplacePOC.Core;

class Program
{
    static void Main()
    {
        string mainQueue = "orders-queue";
        string deadLetterQueue = "orders-dlq";

        using var rabbit = new RabbitMqService();

        // Declare queues (idempotent)
        rabbit.DeclareQueueWithDLQ(mainQueue, deadLetterQueue);

        Console.WriteLine("Publishing 5 sample orders...");

        for (int i = 1; i <= 5; i++)
        {
            var order = new Order
            {
                ProductName = $"Product {i}",
                Quantity = i,
                Price = i * 10
            };

            rabbit.PublishOrder(mainQueue, order);
        }

        Console.WriteLine("Done publishing orders.");
        Console.ReadLine();
    }
}
