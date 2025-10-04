using MarketplacePOC.Core;

class Program
{
    static void Main()
    {
        string mainQueue = "orders-queue";
        string deadLetterQueue = "orders-dlq";

        using var rabbit = new RabbitMqService();

        Console.WriteLine("Starting consumer for main queue...");

        // Consume main queue
        rabbit.ConsumeOrders(mainQueue, order =>
        {
            Console.WriteLine($"Processing Order {order.OrderId}: {order.ProductName} x {order.Quantity} = {order.Price}");

            // Simulate random failure to test DLQ
            if (order.Quantity % 2 == 0)
                throw new Exception("Simulated processing error");
        });

        // Optional: consume dead-letter queue to inspect failed messages
        rabbit.ConsumeOrders(deadLetterQueue, order =>
        {
            Console.WriteLine($"[DLQ] Order {order.OrderId} failed previously: {order.ProductName}");
        });

        Console.WriteLine("Consumer running. Press Ctrl+C to exit.");
        System.Threading.Thread.Sleep(-1); // Keep running
    }
}
