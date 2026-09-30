using FluentAssertions;
using OrderPdf.Domain.Entities;
using OrderPdf.Domain.Enums;
using OrderPdf.Infrastructure.Data;

namespace OrderPdf.Tests;

public class OrderCalculationTests
{
    [Fact]
    public void SimpleOrder_CalculatesFinancialTotalsCorrectly()
    {
        // Arrange
        var orders = SampleDataSeeder.GetSampleOrders();
        var simpleOrder = orders.First(o => o.OrderNumber == "ORD-2026-00125");

        // Act & Assert
        // Item 1: 2 * 1850.00 = 3700.00, Tax 8.5% = 314.50, LineTotal = 4014.50
        // Item 2: 1 * 650.00 = 650.00, Tax 8.5% = 55.25, LineTotal = 705.25
        // Item 3: 4 * 45.00 = 180.00, Tax 8.5% = 15.30, LineTotal = 195.30
        // Subtotal = 3700 + 650 + 180 = 4530.00
        // ItemDiscounts = 0
        // TotalTax = 314.50 + 55.25 + 15.30 = 385.05
        // Shipping = 25.00
        // GrandTotal = 4530.00 - 0 + 385.05 + 25.00 = 4940.05

        simpleOrder.Subtotal.Should().Be(4530.00m);
        simpleOrder.TotalDiscount.Should().Be(0.00m);
        simpleOrder.TotalTax.Should().Be(385.05m);
        simpleOrder.Shipping.Should().Be(25.00m);
        simpleOrder.GrandTotal.Should().Be(4940.05m);
    }

    [Fact]
    public void DiscountedOrder_CalculatesFinancialTotalsCorrectly()
    {
        // Arrange
        var orders = SampleDataSeeder.GetSampleOrders();
        var discountedOrder = orders.First(o => o.OrderNumber == "ORD-2026-00126");

        // Act & Assert
        // Subtotal = (4*3200) + (8*720) + (4*240) + (10*450) = 12800 + 5760 + 960 + 4500 = 24020.00
        discountedOrder.Subtotal.Should().Be(24020.00m);
        
        // Item discounts = 400 + 160 + 40 + 500 = 1100.00; Additional = 200.00; Total = 1300.00
        discountedOrder.ItemDiscounts.Should().Be(1100.00m);
        discountedOrder.TotalDiscount.Should().Be(1300.00m);
        
        discountedOrder.Shipping.Should().Be(150.00m);
        discountedOrder.GrandTotal.Should().Be(discountedOrder.Subtotal - discountedOrder.TotalDiscount + discountedOrder.TotalTax + discountedOrder.Shipping);
    }

    [Fact]
    public void Order_WithZeroItems_ReturnsZeroSubtotalAndGrandTotalEqualsShipping()
    {
        // Arrange
        var emptyOrder = new Order
        {
            OrderNumber = "ORD-EMPTY-001",
            ShippingFee = 15.00m,
            Items = []
        };

        // Act & Assert
        emptyOrder.Subtotal.Should().Be(0m);
        emptyOrder.TotalDiscount.Should().Be(0m);
        emptyOrder.TotalTax.Should().Be(0m);
        emptyOrder.GrandTotal.Should().Be(15.00m);
    }
}
