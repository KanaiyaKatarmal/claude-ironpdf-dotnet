using OrderPdf.Domain.Entities;

namespace OrderPdf.Application.Interfaces;

public interface IOrderPdfService
{
    Task<byte[]> GenerateAsync(Order order, CancellationToken cancellationToken = default);
}
