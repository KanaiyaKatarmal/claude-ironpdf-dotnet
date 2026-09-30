using System.Net;
using OrderPdf.Domain.Entities;

namespace OrderPdf.Application.Models;

public sealed class OrderPdfViewModel
{
    public string OrderId { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public string OrderDateFormatted { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Currency { get; init; } = "USD";
    public string PaymentMethod { get; init; } = string.Empty;

    public CompanyViewModel Company { get; init; } = new();
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerCompanyName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public AddressViewModel BillingAddress { get; init; } = new();
    public AddressViewModel ShippingAddress { get; init; } = new();

    public IReadOnlyList<OrderItemViewModel> Items { get; init; } = [];

    public decimal Subtotal { get; init; }
    public decimal ItemDiscounts { get; init; }
    public decimal AdditionalOrderDiscount { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal TotalTax { get; init; }
    public decimal Shipping { get; init; }
    public decimal GrandTotal { get; init; }

    public string FormattedSubtotal => Subtotal.ToString("C2");
    public string FormattedDiscount => TotalDiscount > 0 ? $"-{TotalDiscount:C2}" : "$0.00";
    public string FormattedTax => TotalTax.ToString("C2");
    public string FormattedShipping => Shipping.ToString("C2");
    public string FormattedGrandTotal => GrandTotal.ToString("C2");

    public string? CustomerNotes { get; init; }
    public int TotalItemCount => Items.Sum(i => i.Quantity);

    public static OrderPdfViewModel FromEntity(Order order)
    {
        var items = order.Items
            .Select((item, index) => OrderItemViewModel.FromEntity(item, index + 1))
            .ToList();

        return new OrderPdfViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDateFormatted = order.OrderDate.ToString("MMMM dd, yyyy"),
            Status = order.Status.ToString(),
            Currency = order.Currency.ToString(),
            PaymentMethod = order.PaymentMethod ?? "Standard Net-30 / Credit Card",
            Company = CompanyViewModel.FromEntity(order.Company),
            CustomerName = order.Customer.Name,
            CustomerCompanyName = order.Customer.CompanyName,
            CustomerEmail = order.Customer.Email,
            CustomerPhone = order.Customer.Phone,
            BillingAddress = AddressViewModel.FromEntity(order.Customer.BillingAddress),
            ShippingAddress = AddressViewModel.FromEntity(order.Customer.ShippingAddress),
            Items = items,
            Subtotal = order.Subtotal,
            ItemDiscounts = order.ItemDiscounts,
            AdditionalOrderDiscount = order.AdditionalOrderDiscount,
            TotalDiscount = order.TotalDiscount,
            TotalTax = order.TotalTax,
            Shipping = order.Shipping,
            GrandTotal = order.GrandTotal,
            CustomerNotes = order.CustomerNotes
        };
    }
}
