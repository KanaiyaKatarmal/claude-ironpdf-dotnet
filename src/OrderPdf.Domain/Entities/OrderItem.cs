namespace OrderPdf.Domain.Entities;

public sealed class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }

    public decimal BasePrice => Quantity * UnitPrice;
    public decimal TotalDiscount => Discount;
    public decimal TaxableAmount => Math.Max(0m, BasePrice - TotalDiscount);
    public decimal TaxAmount => Math.Round(TaxableAmount * (TaxRate / 100m), 2);
    public decimal LineTotal => Math.Round(TaxableAmount + TaxAmount, 2);
}
