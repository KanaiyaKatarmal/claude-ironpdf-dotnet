using OrderPdf.Application.Interfaces;
using OrderPdf.Application.Services;
using OrderPdf.Infrastructure.Data;
using OrderPdf.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Initialize IronPDF Licensing
// The key comes from the environment only. It is never read from a committed
// appsettings.json — see CONTRIBUTING.md.
var licenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

if (!string.IsNullOrWhiteSpace(licenseKey))
{
    IronPdf.License.LicenseKey = licenseKey;
}

// Warm up Chromium at boot so the first user request does not pay the
// initialisation cost, which can run to tens of seconds on a cold process.
IronPdf.Installation.Initialize();

// 2. Register Application & Infrastructure Services
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<ITemplateRenderer, TemplateRenderer>();
builder.Services.AddScoped<IOrderPdfService, OrderPdfService>();

// 3. Add Controllers & OpenAPI Documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// 4. Report IronPDF license status at startup
if (IronPdf.License.IsLicensed)
{
    app.Logger.LogInformation("IronPDF is activated with a valid license key.");
}
else
{
    // Unlicensed behaviour depends on version and trial state: either every page
    // carries a trial watermark, or SaveAs throws "Production use: Requires a
    // license" once the trial grace period expires. Rendering can succeed and
    // saving still fail, so this is louder than a warning in Production.
    var message = "IronPDF has no valid license. Output will be watermarked, and "
                + "SaveAs will throw once the trial grace period expires. "
                + "Set IRONPDF_LICENSE_KEY before starting the application.";

    if (app.Environment.IsProduction())
    {
        app.Logger.LogCritical("{Message}", message);
        throw new InvalidOperationException(message);
    }

    app.Logger.LogWarning("{Message}", message);
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
