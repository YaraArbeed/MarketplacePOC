namespace MarketplacePOC.Core
{
    public class Order
    {
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
