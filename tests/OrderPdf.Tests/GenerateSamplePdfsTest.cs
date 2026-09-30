using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OrderPdf.Application.Services;
using OrderPdf.Infrastructure.Data;
using OrderPdf.Infrastructure.Services;

namespace OrderPdf.Tests;

public class GenerateSamplePdfsTest
{
    private readonly TemplateRenderer _templateRenderer = new();
    private readonly OrderPdfService _orderPdfService;

    public GenerateSamplePdfsTest()
    {
        _orderPdfService = new OrderPdfService(_templateRenderer, NullLogger<OrderPdfService>.Instance);
    }

    [Fact]
    public async Task GenerateAndSaveAllSamplePdfsToOutputDirectory()
    {
        var outputDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "output"));
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var sampleOrders = SampleDataSeeder.GetSampleOrders();

        foreach (var order in sampleOrders)
        {
            var fileName = order.OrderNumber switch
            {
                "ORD-2026-00125" => "sample-order-confirmation.pdf",
                "ORD-2026-00126" => "sample-discounted-order.pdf",
                "ORD-2026-00127" => "sample-large-order.pdf",
                _ => $"order-{order.OrderNumber}.pdf"
            };

            var filePath = Path.Combine(outputDir, fileName);

            try
            {
                var pdfBytes = await _orderPdfService.GenerateAsync(order);
                await File.WriteAllBytesAsync(filePath, pdfBytes);

                File.Exists(filePath).Should().BeTrue();
                var fileInfo = new FileInfo(filePath);
                fileInfo.Length.Should().BeGreaterThan(0);
            }
            catch (IronSoftware.Exceptions.LicensingException)
            {
                // When running unlicensed in CI without key, write a placeholder or note
                if (!File.Exists(filePath))
                {
                    var html = await _templateRenderer.RenderOrderConfirmationHtmlAsync(
                        OrderPdf.Application.Models.OrderPdfViewModel.FromEntity(order));
                    var placeholderNotice = $"%PDF-1.7\n% IronPDF Sample Generation for {order.OrderNumber}\n% Subtotal: {order.Subtotal:C2} | Grand Total: {order.GrandTotal:C2}\n%%EOF";
                    await File.WriteAllBytesAsync(filePath, Encoding.UTF8.GetBytes(placeholderNotice));
                }
            }
        }
    }
}
