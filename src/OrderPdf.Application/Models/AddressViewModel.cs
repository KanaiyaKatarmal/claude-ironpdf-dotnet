using OrderPdf.Domain.Entities;

namespace OrderPdf.Application.Models;

public sealed class AddressViewModel
{
    public string Street { get; init; } = string.Empty;
    public string? SuiteOrApt { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    public string FormattedCityStateZip => $"{City}, {State} {PostalCode}";

    public static AddressViewModel FromEntity(Address? address)
    {
        if (address == null) return new AddressViewModel();
        return new AddressViewModel
        {
            Street = address.Street,
            SuiteOrApt = address.SuiteOrApt,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }
}
