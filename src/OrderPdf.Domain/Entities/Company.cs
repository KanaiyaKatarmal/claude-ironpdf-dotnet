namespace OrderPdf.Domain.Entities;

public sealed class Company
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Acme Commerce Solutions";
    public string? Tagline { get; set; } = "Enterprise Commerce & Cloud Solutions";
    public string? LogoUrl { get; set; }
    public string Email { get; set; } = "billing@acmecommerce.com";
    public string Phone { get; set; } = "+1 (800) 555-0199";
    public string Website { get; set; } = "https://acmecommerce.com";
    public Address Address { get; set; } = new();
    public string TaxId { get; set; } = "US-XX-9876543";
}
