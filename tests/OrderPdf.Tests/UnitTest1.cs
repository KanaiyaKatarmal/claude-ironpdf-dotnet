using FluentAssertions;
using OrderPdf.Infrastructure.Data;

namespace OrderPdf.Tests;

public class SampleDataSeederTests
{
    [Fact]
    public void GetSampleOrders_ContainsThreeDistinctOrders()
    {
        var orders = SampleDataSeeder.GetSampleOrders();

        orders.Should().HaveCount(3);
        orders.Select(o => o.OrderNumber).Should().OnlyHaveUniqueItems();
        orders.First(o => o.OrderNumber == "ORD-2026-00127").Items.Should().HaveCountGreaterThanOrEqualTo(50);
    }
}
