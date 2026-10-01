# Order Confirmation PDF Workflow & Rendering Guide

This document details the HTML-to-PDF rendering pipeline, styling rules, page-break mechanics, and IronPDF engine configurations.

---

## 1. The Rendering Pipeline

```text
Domain Order Entity
        │
        ▼
OrderPdfViewModel (Flattened, Sanitized, Currency-Formatted)
        │
        ▼
TemplateRenderer.cs (Pure HTML5 + Print-Optimized CSS3)
        │
        ▼
IronPdf.ChromePdfRenderer (Chromium Rendering Subsystem)
        │
        ▼
PdfDocument (Vector PDF Document with Dynamic Headers & Footers)
        │
        ▼
HTTP File Stream / PDF File on Disk
```

---

## 2. Multi-Page Layout & CSS Print Rules

Multi-page tables require explicit print CSS properties to avoid awkward line-breaks across page boundaries.

### 2.1 Repeating Table Headers
When an order contains dozens of line items spanning multiple pages, table headers must repeat at the top of every subsequent page:

```css
.items-table thead {
    display: table-header-group;
}
```

### 2.2 Avoiding Row Splitting
Individual rows must not be split horizontally in half across a page boundary:

```css
.items-table tbody tr {
    page-break-inside: avoid;
    break-inside: avoid;
}
```

### 2.3 Keeping Summary & Notes Intact
The financial totals box and customer notes block should not be split across pages:

```css
.summary-container {
    page-break-inside: avoid;
    break-inside: avoid;
}
```

---

## 3. Dynamic Headers, Footers & Page Numbering

IronPDF supports dynamic tokens inside `HtmlHeaderFooter` fragments:
- `{page}`: Current page number
- `{total-pages}`: Total document page count
- `{date}`: Current rendering date
- `{time}`: Current rendering time
- `{url}`: Source URL of the rendered page
- `{html-title}`: Title of the source HTML document
- `{pdf-title}`: Title set on the PDF document

### Code Configuration in `OrderPdfService.cs`:

```csharp
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
    <div style='font-family: sans-serif; font-size: 8.5px; color: #64748b; width: 100%; border-top: 1px solid #e2e8f0; padding-top: 4px; display: flex; justify-content: space-between;'>
        <span>Thank you for your business &bull; https://acmecommerce.com</span>
        <span>Page {page} of {total-pages}</span>
    </div>",
    MaxHeight = 12,
    DrawDividerLine = false
};
```

---

## 4. Production Considerations

### 4.1 Resource Management
IronPDF's `PdfDocument` implements `IDisposable`. Always wrap it in a `using` statement or return `.BinaryData` immediately:

```csharp
using var pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
return pdf.BinaryData;
```

### 4.2 Security & Injection Protection
All user and business data interpolated into the HTML template is passed through `System.Net.WebUtility.HtmlEncode()`. Untrusted user input (e.g. order notes, customer names) cannot execute arbitrary JavaScript or break the document structure.

### 4.3 High-Volume Performance
For applications generating thousands of PDFs per hour:

1. **Warm up Chromium at startup.** `Program.cs` calls
   `IronPdf.Installation.Initialize()` after the license key is applied. The first
   render in a cold process initialises Chromium and can take tens of seconds; this
   pays that cost at boot instead of on a user's first request.
2. **Know what "reuse the renderer" does and does not mean here.** Reusing a
   `ChromePdfRenderer` is sound advice where `RenderingOptions` is invariant. This
   project writes the order number into the page header, so the renderer is built per
   request by design — a shared instance would race across concurrent requests and
   print one customer's details onto another customer's PDF. If profiling ever shows
   construction to be material, pool the renderers and rent one exclusively per
   render; do not promote the existing instance to a singleton.
3. **Offload bulk generation.** Push batch work to a background worker or message
   queue (RabbitMQ, Azure Service Bus) rather than holding an HTTP request thread
   open for the duration of a render.
4. **Dispose documents.** `PdfDocument` is `IDisposable` and holds unmanaged
   resources. In a long-running process this is not optional.

### 4.4 Fonts

Chromium renders with the fonts installed on the host. A container with no font
packages produces blank boxes where text should be, with no error and no warning —
one of the most common IronPDF support tickets. Install `fonts-liberation` and
`fonts-dejavu-core` in the image, or embed web fonts via `RenderingOptions.CustomCssUrl`.
See [deployment.md](deployment.md).

### 4.5 Archival Output

Order confirmations are often retained for years. If this document needs to survive
a compliance audit, render to PDF/A rather than plain PDF:

```csharp
pdf.SaveAsPdfA("order-archive.pdf", IronPdf.PdfAVersions.PdfA3b);
```

`SaveAsPdfUA` produces a tagged, screen-reader-accessible document, which is a
procurement requirement in public-sector contracts.
