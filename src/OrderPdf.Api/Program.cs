using OrderPdf.Application.Interfaces;
using OrderPdf.Application.Services;
using OrderPdf.Infrastructure.Data;
using OrderPdf.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Initialize IronPDF Licensing
var licenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
                 ?? builder.Configuration["IronPdf:LicenseKey"];

if (!string.IsNullOrWhiteSpace(licenseKey))
{
    IronPdf.License.LicenseKey = licenseKey;
}

// 2. Register Application & Infrastructure Services
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<ITemplateRenderer, TemplateRenderer>();
builder.Services.AddScoped<IOrderPdfService, OrderPdfService>();

// 3. Add Controllers & OpenAPI Documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// 4. Check & Log IronPDF License Status
if (IronPdf.License.IsLicensed)
{
    app.Logger.LogInformation("IronPDF is activated with a valid license key.");
}
else
{
    app.Logger.LogWarning("IronPDF is running in unlicensed / trial mode. Generated PDFs will include a watermark.");
}

// 5. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the implicit Program class public for WebApplicationFactory in integration tests
public partial class Program { }
