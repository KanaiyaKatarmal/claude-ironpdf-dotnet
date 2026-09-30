using System.Net;
using IronPdf;
using IronPdf.Rendering;
using Microsoft.Extensions.Logging;
using OrderPdf.Application.Interfaces;
using OrderPdf.Application.Models;
using OrderPdf.Domain.Entities;

namespace OrderPdf.Infrastructure.Services;

public sealed class OrderPdfService : IOrderPdfService
{
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ILogger<OrderPdfService> _logger;

    public OrderPdfService(ITemplateRenderer templateRenderer, ILogger<OrderPdfService> logger)
    {
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task<byte[]> GenerateAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Starting PDF generation for Order {OrderNumber} with {ItemCount} items",
            order.OrderNumber, order.Items.Count);

        var viewModel = OrderPdfViewModel.FromEntity(order);
        var htmlContent = await _templateRenderer.RenderOrderConfirmationHtmlAsync(viewModel, cancellationToken);

        var renderer = new ChromePdfRenderer();

        // Print & Layout Settings
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        renderer.RenderingOptions.MarginTop = 18;       // mm
        renderer.RenderingOptions.MarginBottom = 22;    // mm
        renderer.RenderingOptions.MarginLeft = 14;      // mm
        renderer.RenderingOptions.MarginRight = 14;     // mm
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
        renderer.RenderingOptions.CreatePdfFormsFromHtml = false;

        // Header Configuration
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = $$"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; font-size: 8.5px; color: #64748b; width: 100%; border-bottom: 1px solid #e2e8f0; padding-bottom: 4px; display: flex; justify-content: space-between; align-items: center;">
                <span>{{WebUtility.HtmlEncode(viewModel.Company.Name)}} &bull; Order Confirmation</span>
                <span>Order #{{WebUtility.HtmlEncode(viewModel.OrderNumber)}}</span>
            </div>
            """,
            MaxHeight = 12,
            DrawDividerLine = false
        };

        // Footer Configuration with Dynamic Page Numbers
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = $$"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; font-size: 8.5px; color: #64748b; width: 100%; border-top: 1px solid #e2e8f0; padding-top: 4px; display: flex; justify-content: space-between; align-items: center;">
                <span>Thank you for your business &bull; {{WebUtility.HtmlEncode(viewModel.Company.Website)}}</span>
                <span>Page {page} of {total-pages}</span>
            </div>
            """,
            MaxHeight = 12,
            DrawDividerLine = false
        };

        using var pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
        var pdfBytes = pdf.BinaryData;

        _logger.LogInformation("Successfully generated PDF for Order {OrderNumber} ({ByteCount} bytes)",
            order.OrderNumber, pdfBytes.Length);

        return pdfBytes;
    }
}
