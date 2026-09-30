namespace OrderPdf.Domain.Entities;

public sealed class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DefaultUnitPrice { get; set; }
}
