# 🛒 Marketplace POC — RabbitMQ Quorum Queue with DLQ

This is a Proof of Concept (POC) demonstrating **RabbitMQ Quorum Queues** with a **Dead Letter Queue (DLQ)** using a clean .NET 9.0 modular structure.

---

## 📂 Project Structure

```
MarketplacePOC/
├─ MarketplacePOC.sln
├─ MarketplacePOC.Core/       # Shared RabbitMQ service & DTOs
├─ MarketplacePOC.Publisher/  # Produces orders and sends them to RabbitMQ
└─ MarketplacePOC.Consumer/   # Consumes and processes orders from RabbitMQ
```

---

## 🧠 Key Features

* ✅ Quorum Queue (for reliable message persistence)
* ⚰️ Dead Letter Queue (DLQ) for failed or rejected messages
* 💡 Publisher/Consumer separation
* 🔄 Auto-acknowledgment and error handling
* ⚙️ Configurable queue and DLQ setup

---

## 🧩 Components

### **1️⃣ MarketplacePOC.Core**

Contains:

* `RabbitMqService` → Handles connection, queue declaration, publishing, and consuming
* `Order` DTO → Represents a simple order message

### **2️⃣ MarketplacePOC.Publisher**

Simulates order creation:

```csharp
using MarketplacePOC.Core;

class Program
{
    static void Main()
    {
        string mainQueue = "orders-queue";
        string deadLetterQueue = "orders-dlq";

        using var rabbit = new RabbitMqService();

        // Create queues
        rabbit.DeclareQueueWithDLQ(mainQueue, deadLetterQueue);

        Console.WriteLine("Publishing 5 orders...");
        for (int i = 1; i <= 5; i++)
        {
            rabbit.PublishOrder(mainQueue, new Order
            {
                OrderId = Guid.NewGuid(),
                ProductName = $"Product {i}",
                Quantity = i,
                Price = i * 10
            });
        }
    }
}
```

### **3️⃣ MarketplacePOC.Consumer**

Consumes and processes messages:

```csharp
using MarketplacePOC.Core;

class Program
{
    static void Main()
    {
        string mainQueue = "orders-queue";
        string deadLetterQueue = "orders-dlq";

        using var rabbit = new RabbitMqService();

        rabbit.ConsumeOrders(mainQueue, order =>
        {
            Console.WriteLine($"Processing Order {order.OrderId}: {order.ProductName} x {order.Quantity}");
            
            if (order.Quantity % 2 == 0)
                throw new Exception("Simulated processing error");
        });

        rabbit.ConsumeOrders(deadLetterQueue, order =>
        {
            Console.WriteLine($"[DLQ] Order {order.OrderId} failed previously: {order.ProductName}");
        });

        Console.WriteLine("Consumer running... Press Ctrl+C to exit.");
        Thread.Sleep(-1);
    }
}
```

---

## 🚀 How to Run

1. Make sure RabbitMQ is running locally:

   ```bash
   docker run -d --hostname my-rabbit --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management
   ```

   Access management UI → [http://localhost:15672](http://localhost:15672)
   (user: `guest`, password: `guest`)

2. Run the **Consumer** project first → starts listening for messages.

3. Run the **Publisher** project → sends test orders.

4. Observe the output in both terminals:

   * Normal orders are processed successfully.
   * Orders with errors go to the **DLQ**.

---

## 🧪 Example Output

### **Publisher**

```
Declared quorum queue: orders-queue with DLQ orders-dlq
Published Order 1
Published Order 2
Published Order 3
Published Order 4
Published Order 5
```

### **Consumer**

```
Processing Order {GUID}: Product 1 x 1
Processing Order {GUID}: Product 2 x 2
Error: Simulated processing error, sending to DLQ
[DLQ] Order {GUID} failed previously: Product 2
Processing Order {GUID}: Product 3 x 3
...
```

---

## 🧰 Tech Stack

* .NET 9.0
* RabbitMQ 3.13 (or later)
* `RabbitMQ.Client` NuGet package
* Modular clean architecture



