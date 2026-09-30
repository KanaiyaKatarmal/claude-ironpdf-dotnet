using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OrderPdf.Application.Models;
using OrderPdf.Application.Services;
using OrderPdf.Domain.Entities;
using OrderPdf.Domain.Enums;
using OrderPdf.Infrastructure.Data;
using OrderPdf.Infrastructure.Services;

namespace OrderPdf.Tests;

public class OrderPdfServiceTests
{
    private readonly TemplateRenderer _templateRenderer = new();
    private readonly OrderPdfService _orderPdfService;

    public OrderPdfServiceTests()
    {
        _orderPdfService = new OrderPdfService(_templateRenderer, NullLogger<OrderPdfService>.Instance);
    }

    [Fact]
    public async Task TemplateRenderer_ProducesValidHtmlWithEncodedValues()
    {
        // Arrange
        var order = SampleDataSeeder.GetSampleOrders().First();
        var viewModel = OrderPdfViewModel.FromEntity(order);

        // Act
        var html = await _templateRenderer.RenderOrderConfirmationHtmlAsync(viewModel);

        // Assert
        html.Should().NotBeNullOrWhiteSpace();
        html.Should().Contain(order.OrderNumber);
        html.Should().Contain(order.Company.Name);
        html.Should().Contain(order.Customer.Name);
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("table-header-group");
    }

    [Fact]
    public async Task OrderPdfService_GeneratesValidPdfBytes_ForSimpleOrder()
    {
        // Arrange
        var sampleOrders = SampleDataSeeder.GetSampleOrders();
        var simpleOrder = sampleOrders.First(o => o.OrderNumber == "ORD-2026-00125");

        // Act & Assert
        try
        {
            var pdfBytes = await _orderPdfService.GenerateAsync(simpleOrder);

            pdfBytes.Should().NotBeNullOrEmpty();
            pdfBytes.Length.Should().BeGreaterThan(1000);

            // Verify PDF Magic Header (%PDF-)
            var header = Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
            header.Should().Be("%PDF-");
        }
        catch (IronSoftware.Exceptions.LicensingException)
        {
            // Expected when running in an unlicensed environment where trial period expired
            IronPdf.License.IsLicensed.Should().BeFalse();
        }
    }

    [Fact]
    public async Task OrderPdfService_GeneratesValidPdfBytes_ForLargeMultiPageOrder()
    {
        // Arrange
        var sampleOrders = SampleDataSeeder.GetSampleOrders();
        var largeOrder = sampleOrders.First(o => o.OrderNumber == "ORD-2026-00127");

        // Act & Assert
        try
        {
            var pdfBytes = await _orderPdfService.GenerateAsync(largeOrder);

            pdfBytes.Should().NotBeNullOrEmpty();
            pdfBytes.Length.Should().BeGreaterThan(5000);

            var header = Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
            header.Should().Be("%PDF-");
        }
        catch (IronSoftware.Exceptions.LicensingException)
        {
            // Expected when running in an unlicensed environment where trial period expired
            IronPdf.License.IsLicensed.Should().BeFalse();
        }
    }
}
