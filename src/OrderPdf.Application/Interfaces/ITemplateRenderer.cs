using OrderPdf.Application.Models;

namespace OrderPdf.Application.Interfaces;

public interface ITemplateRenderer
{
    Task<string> RenderOrderConfirmationHtmlAsync(OrderPdfViewModel model, CancellationToken cancellationToken = default);
}
