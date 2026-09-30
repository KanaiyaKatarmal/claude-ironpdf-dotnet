namespace OrderPdf.Domain.Entities;

public sealed class Address
{
    public string Street { get; set; } = string.Empty;
    public string? SuiteOrApt { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public string FormattedInline =>
        string.IsNullOrWhiteSpace(SuiteOrApt)
            ? $"{Street}, {City}, {State} {PostalCode}, {Country}"
            : $"{Street}, {SuiteOrApt}, {City}, {State} {PostalCode}, {Country}";
}
