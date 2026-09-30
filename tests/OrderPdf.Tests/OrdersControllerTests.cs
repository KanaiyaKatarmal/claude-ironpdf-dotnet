using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using OrderPdf.Api.Controllers;
using OrderPdf.Application.Interfaces;
using OrderPdf.Application.Services;
using OrderPdf.Domain.Entities;
using OrderPdf.Infrastructure.Data;
using OrderPdf.Infrastructure.Services;

namespace OrderPdf.Tests;

public class OrdersControllerTests
{
    private readonly InMemoryOrderRepository _repository;
    private readonly FakeOrderPdfService _fakePdfService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _repository = new InMemoryOrderRepository();
        _fakePdfService = new FakeOrderPdfService();
        _controller = new OrdersController(_repository, _fakePdfService, NullLogger<OrdersController>.Instance);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOkWithPreseededOrders()
    {
        // Act
        var result = await _controller.GetAllOrders(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var orders = okResult.Value.Should().BeAssignableTo<IReadOnlyList<Order>>().Subject;
        orders.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetOrderById_WhenExists_ReturnsOkWithOrder()
    {
        // Act
        var result = await _controller.GetOrderById("ORD-2026-00125", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var order = okResult.Value.Should().BeOfType<Order>().Subject;
        order.OrderNumber.Should().Be("ORD-2026-00125");
    }

    [Fact]
    public async Task GetOrderById_WhenNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetOrderById("NON-EXISTENT-ID", CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DownloadOrderPdf_WhenOrderNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DownloadOrderPdf("INVALID-ORDER-ID", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DownloadOrderPdf_WhenOrderExists_ReturnsFileContentResultWithPdfContentType()
    {
        // Act
        var result = await _controller.DownloadOrderPdf("ORD-2026-00125", CancellationToken.None);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("Order-ORD-2026-00125.pdf");
        fileResult.FileContents.Should().NotBeNullOrEmpty();
    }

    private sealed class FakeOrderPdfService : IOrderPdfService
    {
        public Task<byte[]> GenerateAsync(Order order, CancellationToken cancellationToken = default)
        {
            // Simulated valid %PDF- bytes for controller testing without external runtime dependencies
            var fakePdfBytes = "%PDF-1.7 Fake PDF content for testing"u8.ToArray();
            return Task.FromResult(fakePdfBytes);
        }
    }
}
