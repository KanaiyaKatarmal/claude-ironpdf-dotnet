# CLAUDE.md

Project memory for Claude Code. Keep it short — this file is loaded into every session.

## What this is

An ASP.NET Core 10 Web API that renders multi-page Order Confirmation PDFs with
IronPDF, built to demonstrate the Claude Code + Claude Skills + IronPDF workflow.

## IronPDF knowledge lives in the skill, not here

`.claude/skills/ironpdf/SKILL.md` is the official IronPDF skill, vendored from
ironpdf.com. Claude loads it automatically for IronPDF work.

**Never paste the skill's contents into this file.** A root copy of it existed
until it was removed — it doubled the context for no gain and silently went stale.
See `.claude/skills/ironpdf/SOURCE.md` for provenance and the refresh command.

## Commands

```bash
dotnet build
dotnet test
dotnet run --project src/OrderPdf.Api     # https://localhost:7000
```

The app serves `src/OrderPdf.Api/wwwroot/index.html` at the root and the OpenAPI
document at `/openapi/v1.json` in Development. **There is no Swagger UI** — the
project uses `AddOpenApi()`/`MapOpenApi()`, not Swashbuckle.

## Where things live

| Change | File |
|---|---|
| Renderer settings, headers, footers, margins | `src/OrderPdf.Infrastructure/Services/OrderPdfService.cs` |
| PDF HTML and print CSS | `src/OrderPdf.Application/Services/TemplateRenderer.cs` |
| Money and totals | `src/OrderPdf.Domain/Entities/Order.cs` |
| Sample orders | `src/OrderPdf.Infrastructure/Data/SampleDataSeeder.cs` |
| Endpoints | `src/OrderPdf.Api/Controllers/OrdersController.cs` |

## Conventions

- All layout is HTML + CSS rendered by Chromium. Do not reach for a drawing API.
- Everything interpolated into the template goes through `WebUtility.HtmlEncode`.
  No exceptions — order notes and customer names are untrusted.
- Money is `decimal`. Never `double`, never `float`.
- Page-break CSS (`table-header-group`, `page-break-inside: avoid`) is load-bearing
  for the 65-item sample. Changing it means re-checking `output/sample-large-order.pdf`.
- Keep `OrderPdf.Domain` free of any framework or IronPDF reference.
- `OrderPdfService` builds a `ChromePdfRenderer` per request on purpose. Its
  `RenderingOptions` carry per-order header and footer values, so a shared instance
  would race across concurrent requests. Do not "optimise" it into a singleton.

## Licensing

The key comes from the `IRONPDF_LICENSE_KEY` environment variable. Never write a key
into `appsettings.json`, a test, or terminal output. Unlicensed output is watermarked
and `SaveAs` can throw once the trial grace period expires, so say so rather than
presenting watermarked output as finished.

## Tests

`dotnet test` renders real PDFs via Chromium, so the suite is slow on a cold run
and needs the IronPDF native binaries. `GenerateSamplePdfsTest` rewrites
`output/*.pdf` — regenerate those deliberately, not as a side effect.
