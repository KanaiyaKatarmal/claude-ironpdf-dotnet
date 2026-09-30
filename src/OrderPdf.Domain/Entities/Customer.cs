namespace OrderPdf.Domain.Entities;

public sealed class Customer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Address BillingAddress { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
}
