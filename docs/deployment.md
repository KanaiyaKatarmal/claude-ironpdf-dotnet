# Deployment

This repository ships a **Windows-only** package reference. `OrderPdf.Infrastructure.csproj`
contains a single `<PackageReference Include="IronPdf" />`, which carries the Windows
Chromium binaries. On Linux, in Docker, or on Apple Silicon it will fail at the first
render. This page covers what to change.

## Pick the package for the target platform

Every Iron package in one solution must share the **same version**.

| Target | Package |
|---|---|
| Windows | `IronPdf` |
| Linux x64, including most Docker images | `IronPdf.Linux` |
| Linux ARM64 | `IronPdf.Linux.ARM` |
| macOS Intel | `IronPdf.MacOs` |
| macOS Apple Silicon | `IronPdf.MacOs.ARM` |
| Natives supplied another way | `IronPdf.Slim` |

For a project that has to build on more than one platform, condition the reference:

```xml
<ItemGroup>
  <PackageReference Include="IronPdf"       Version="2026.9.2" Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
  <PackageReference Include="IronPdf.Linux" Version="2026.9.2" Condition="$([MSBuild]::IsOSPlatform('Linux'))" />
  <PackageReference Include="IronPdf.MacOs" Version="2026.9.2" Condition="$([MSBuild]::IsOSPlatform('OSX'))" />
</ItemGroup>
```

`IronPdf.Slim` plus `Installation.AutomaticallyDownloadNativeBinaries = true` is the
alternative, but it needs outbound network access and a writable deployment directory
on first run. In a locked-down container, prefer the platform package.

## Warm up Chromium at startup

The first render in a fresh process initialises Chromium and can take tens of seconds.
Pay that cost at boot rather than on a user's first request:

```csharp
IronPdf.Installation.Initialize();
```

`Program.cs` calls this after the license key is applied.

## Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Bake the Chromium dependencies into the image. Doing this at runtime via
# LinuxAndDockerDependenciesAutoConfig costs 2-3 minutes on first start and needs root.
RUN apt-get update && apt-get install -y --no-install-recommends \
      libgdiplus libc6-dev libx11-6 libx11-xcb1 libxcb1 libxcomposite1 \
      libxcursor1 libxdamage1 libxext6 libxi6 libxtst6 libnss3 libcups2 \
      libxss1 libxrandr2 libatk1.0-0 libatk-bridge2.0-0 libgtk-3-0 \
      libasound2 libpangocairo-1.0-0 libgbm1 \
      fonts-liberation fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

ENV IRONPDF_TEMP=/tmp/ironpdf
RUN mkdir -p /tmp/ironpdf && chmod 777 /tmp/ironpdf
```

`fonts-liberation` and `fonts-dejavu-core` are not optional. A container with no
installed fonts renders text as blank boxes with no error, which is one of the most
common IronPDF support tickets.

That package list targets the `mcr.microsoft.com/dotnet/aspnet:10.0` base image. A
different base — Alpine, distroless, a hardened corporate image — will need a
different set, so check what Chromium reports missing on first run rather than
assuming this list transfers.

Configure the writable temp path at startup:

```csharp
IronPdf.Installation.TempFolderPath = Environment.GetEnvironmentVariable("IRONPDF_TEMP")
                                      ?? Path.Combine(Path.GetTempPath(), "ironpdf");
```

## Host-specific notes

| Host | What to do |
|---|---|
| Azure App Service (Linux) | Use a **paid** tier. The free and shared tiers block the browser process. Set `Installation.TempFolderPath` to a writable path. |
| AWS Lambda | Container image only — the zip size limit and native dependencies rule out plain zip deploys. Allow at least 1 GB memory and point `TempFolderPath` at `/tmp`. |
| IIS | The app-pool identity needs a writable temp and deployment directory, and "Load User Profile" enabled. |
| Low-memory containers | `Installation.SingleProcess = true`, `Installation.ChromeGpuMode = ChromeGpuModes.Disabled`, and bound the pool with `Installation.ChromeBrowserLimit`. |
| Separate render host | `Installation.ConnectToIronPdfHost(...)` points an `IronPdf.Slim` client at a standalone IronPdfEngine container. |

## Throughput

- Call `Installation.Initialize()` at boot. This is the large, one-time cost, and it
  is shared process-wide.
- Use the `...Async` render methods on request threads.
- `PdfDocument` is `IDisposable`. Dispose it in long-running processes.
- For bulk generation, push the work to a background worker or queue rather than
  holding an HTTP request open.
- On renderer reuse, read the comment in `OrderPdfService.GenerateAsync` first.
  Reusing a `ChromePdfRenderer` is sound advice **only where `RenderingOptions` is
  invariant**. This project writes per-order values into the page header, so a shared
  instance would race across concurrent requests. If profiling ever shows renderer
  construction to be material here, pool them and rent one exclusively per render —
  do not promote the existing instance to a singleton.

## Licensing in a deployed environment

Supply the key through the hosting platform's secret store as the environment
variable `IRONPDF_LICENSE_KEY`. Never bake it into an image layer, an
`appsettings.json`, or a build log.

Unlicensed behaviour depends on version and trial state: either every page carries
a trial watermark, or `SaveAs` throws `Production use: Requires a license` once the
trial grace period expires. Rendering can succeed and saving still fail, so check
`IronPdf.License.IsLicensed` at startup and fail loudly rather than shipping
watermarked PDFs to customers.

---

*Deployment guidance contributed by Iron Software.*
