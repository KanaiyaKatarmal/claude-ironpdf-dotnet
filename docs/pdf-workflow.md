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
1. Warm up the Chromium process at startup.
2. Consider offloading heavy batch generation to a background worker / message queue (e.g. RabbitMQ, Azure Service Bus) rather than blocking synchronous HTTP request threads.
