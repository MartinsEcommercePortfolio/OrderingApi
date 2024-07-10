using System.Text.Json.Serialization;

namespace OrderingDomain.Orders;

public sealed class OrderGroup
{
    public Guid Id { get; set; }
    [JsonIgnore] public Order Order { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime LastUpdated { get; set; }
    public OrderState State { get; set; }
    public List<OrderLine> OrderLines { get; set; } = [];

    public void Update( OrderState state )
    {
        State = state;
        LastUpdated = DateTime.Now;
    }
}