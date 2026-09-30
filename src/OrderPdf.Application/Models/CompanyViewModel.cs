using OrderPdf.Domain.Entities;

namespace OrderPdf.Application.Models;

public sealed class CompanyViewModel
{
    public string Name { get; init; } = string.Empty;
    public string Tagline { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Website { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public AddressViewModel Address { get; init; } = new();

    public static CompanyViewModel FromEntity(Company? company)
    {
        if (company == null) return new CompanyViewModel();
        return new CompanyViewModel
        {
            Name = company.Name,
            Tagline = company.Tagline ?? string.Empty,
            Email = company.Email,
            Phone = company.Phone,
            Website = company.Website,
            TaxId = company.TaxId,
            Address = AddressViewModel.FromEntity(company.Address)
        };
    }
}
