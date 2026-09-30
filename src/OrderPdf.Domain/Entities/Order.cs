using OrderPdf.Domain.Enums;

namespace OrderPdf.Domain.Entities;

public sealed class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderNumber { get; set; } = string.Empty;
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Confirmed;
    public CurrencyCode Currency { get; set; } = CurrencyCode.USD;

    public Company Company { get; set; } = new();
    public Customer Customer { get; set; } = new();
    public List<OrderItem> Items { get; set; } = [];

    public decimal ShippingFee { get; set; }
    public decimal AdditionalOrderDiscount { get; set; }
    public string? CustomerNotes { get; set; }
    public string? InternalNotes { get; set; }
    public string? PaymentMethod { get; set; } = "Credit Card (ending 4242)";

    public decimal Subtotal => Items.Sum(i => i.BasePrice);
    public decimal ItemDiscounts => Items.Sum(i => i.TotalDiscount);
    public decimal TotalDiscount => ItemDiscounts + AdditionalOrderDiscount;
    public decimal TotalTax => Items.Sum(i => i.TaxAmount);
    public decimal Shipping => ShippingFee;
    public decimal GrandTotal => Math.Round(Subtotal - TotalDiscount + TotalTax + Shipping, 2);
}
