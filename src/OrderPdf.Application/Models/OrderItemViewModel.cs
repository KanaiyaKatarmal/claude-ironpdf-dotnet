using OrderPdf.Domain.Entities;

namespace OrderPdf.Application.Models;

public sealed class OrderItemViewModel
{
    public int Index { get; init; }
    public string SKU { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Discount { get; init; }
    public decimal TaxRate { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal LineTotal { get; init; }

    public string FormattedUnitPrice => UnitPrice.ToString("C2");
    public string FormattedDiscount => Discount > 0 ? $"-{Discount:C2}" : "$0.00";
    public string FormattedTax => TaxAmount > 0 ? $"{TaxAmount:C2} ({TaxRate:0.##}%)" : "$0.00";
    public string FormattedLineTotal => LineTotal.ToString("C2");

    public static OrderItemViewModel FromEntity(OrderItem item, int index)
    {
        return new OrderItemViewModel
        {
            Index = index,
            SKU = item.SKU,
            ProductName = item.ProductName,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Discount = item.Discount,
            TaxRate = item.TaxRate,
            TaxAmount = item.TaxAmount,
            LineTotal = item.LineTotal
        };
    }
}
