---
name: ironpdf
description: >
  Create, edit, convert, secure and read PDF files in C#/.NET using IronPDF (the `IronPdf`
  NuGet package). Use when the task involves HTML/URL/Markdown/RTF/DOCX/image to PDF,
  merging or splitting PDFs, page manipulation, stamps, watermarks, headers and footers,
  AcroForm fields, passwords and permissions, digital signatures, redaction, extracting
  text/images/tables, rasterizing pages to PNG/JPEG/TIFF, compression, PDF/A or PDF/UA
  conversion, printing — or whenever a project already references `IronPdf`,
  `ChromePdfRenderer`, or `PdfDocument`.
---

# IronPDF (C# / .NET)

IronPDF renders PDFs with an embedded Chromium engine: **anything you can express in HTML
and CSS, you can produce as a pixel-accurate PDF**, and it also edits, secures and reads
existing PDFs. Prefer HTML+CSS for layout — it is the shortest path to a correct document.

## Scope of this skill

| | |
|---|---|
| Package | `IronPdf` (plus a platform variant, see Install) |
| Versions | 2023.x – 2026.x (`IronPdf.Slim` carries the managed assembly) |
| Namespaces | `IronPdf`, `IronPdf.Rendering`, `IronPdf.Editing`, `IronPdf.Security`, `IronPdf.Signing`, `IronPdf.Extractions`, `IronPdf.MetaData`, `IronSoftware.Forms` |
| Runtimes | .NET Framework 4.6.2+, .NET Standard 2.0+, .NET Core 2.0+, .NET 5–10 |

**Not covered: the `IronPdf.Universal` package.** That is a different product line with an
incompatible facade API (`Create.FromHtml(...)`). If the project references
`IronPdf.Universal`, say so and stop — do not translate the calls below into it, and never
mix the two packages in one project.

## Install — pick the package for the target platform

All Iron packages in one solution must share the **same version**.

| Target | Package |
|---|---|
| Windows | `IronPdf` |
| Linux x64 (incl. most Docker) | `IronPdf.Linux` |
| Linux ARM64 | `IronPdf.Linux.ARM` |
| macOS Intel | `IronPdf.MacOs` |
| macOS Apple Silicon | `IronPdf.MacOs.ARM` |
| Natives supplied another way | `IronPdf.Slim` |

```bash
dotnet add package IronPdf          # swap for the row above that matches the runtime
```

Cross-platform projects: reference the platform packages conditionally on
`$(RuntimeIdentifier)` / an MSBuild condition, or use `IronPdf.Slim` and set
`Installation.AutomaticallyDownloadNativeBinaries = true` so the Chromium binaries are
fetched on first run (needs outbound network and a writable deployment directory — it will
fail in a locked-down container, so prefer the platform package there).

## Licensing — do this first, every time

```csharp
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
// Installation.LicenseKey is a synonym. Set it once at startup, before any rendering.
if (!IronPdf.License.IsLicensed)
    Console.Error.WriteLine("IronPDF is unlicensed — output will be watermarked or refused.");
```

Rules:

- Read the key from the environment (`IRONPDF_LICENSE_KEY`) or user secrets. **Never** inline
  a key in source, never write one into a file you commit, never echo one to the terminal.
- **Treat a missing key as a blocker, not a warning.** Unlicensed behaviour depends on version
  and trial state: either every page carries a trial watermark, or `SaveAs` throws outright
  (`Production use: Requires a license`, after the trial grace period expires). Rendering can
  succeed and *saving* still fail. If `IsLicensed` is false, say so and ask the user for a key
  rather than producing output they cannot use. Trial keys: <https://ironpdf.com>.
- `IronPdf.License.IsValidLicense(key)` checks a key without applying it.

## Running a one-off PDF task from the terminal

Requires only the .NET SDK. On **.NET 10+**, a single file is the whole program — no project,
no `.csproj`:

```bash
cat > /tmp/task.cs <<'EOF'
#:package IronPdf@2026.7.2
#:property PublishAot=false
using IronPdf;
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
new ChromePdfRenderer().RenderHtmlFileAsPdf("invoice.html").SaveAs("invoice.pdf");
EOF
dotnet run /tmp/task.cs
```

`#:property PublishAot=false` is **required**: .NET 10 file-based apps default to Native AOT,
and IronPDF needs runtime code generation. Without it the renderer's constructor throws
`PlatformNotSupportedException: Dynamic code generation is not supported on this platform`.
Swap the package for the platform row above when not on Windows.

On older SDKs, use a scratch project — and **reuse the same scratch directory** for later
tasks, because the NuGet restore (Chromium natives, ~100 MB) is the slow part:

```bash
dotnet new console -o /tmp/ironpdf-scratch && cd /tmp/ironpdf-scratch
dotnet add package IronPdf
# write Program.cs, then:
dotnet run
```

First render in a fresh environment can take tens of seconds while Chromium initialises;
subsequent renders in the same process are fast. When the user is building a feature rather
than asking for a one-off artifact, skip the scratch project and write the same calls into
their application instead.

## Recipes

Every call below is verified against the shipped assembly.

### HTML / URL / Markdown / RTF / DOCX / image → PDF

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;          // millimetres
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;

PdfDocument a = renderer.RenderHtmlAsPdf("<h1>Hello</h1>", baseUrl: "file:///assets/");
PdfDocument b = renderer.RenderHtmlFileAsPdf("invoice.html");   // relative assets resolve
PdfDocument c = renderer.RenderUrlAsPdf("https://example.com");
PdfDocument d = renderer.RenderMarkdownFileAsPdf("README.md");
PdfDocument e = renderer.RenderRtfFileAsPdf("letter.rtf");
PdfDocument f = renderer.RenderZipFileAsPdf("site.zip", "index.html");
a.SaveAs("out.pdf");
```

`ChromePdfRenderer` also has `...Async` variants of each (`RenderHtmlAsPdfAsync`,
`RenderUrlAsPdfAsync`, …) — use them in web apps. Static one-shot equivalents exist as
`StaticRenderHtmlAsPdf` etc.

```csharp
new DocxToPdfRenderer().RenderDocxAsPdf("contract.docx").SaveAs("contract.pdf");
ImageToPdfConverter.ImageToPdf(new[] { "1.png", "2.png" }).SaveAs("scans.pdf");
renderer.RenderHtmlFileAsPdfUA("report.html").SaveAs("accessible.pdf");   // tagged PDF/UA
```

**Rendering options worth knowing** (all on `renderer.RenderingOptions`):
`PaperSize`, `PaperOrientation`, `SetCustomPaperSizeInInches` /
`SetCustomPaperSizeinMilimeters`, `MarginTop/Bottom/Left/Right`, `FitToPaperMode`,
`ForcePaperSize`, `Zoom`, `DPI`, `GrayScale`, `JpegQuality`, `PrintHtmlBackgrounds`,
`CssMediaType`, `EnableJavaScript`, `Javascript`, `RenderDelay`, `WaitFor`, `Timeout`,
`ViewPortWidth/Height`, `CustomCssUrl`, `CustomCookies`, `HttpRequestHeaders`,
`InputEncoding`, `Title`, `FirstPageNumber`, `TextHeader`/`TextFooter`,
`HtmlHeader`/`HtmlFooter`, `TableOfContents`, `AutoBookmarksFromHeadings`,
`CreatePdfFormsFromHtml`, `EnableMathematicalLaTex`.

JavaScript-driven pages: set `EnableJavaScript = true` and wait deterministically with
`RenderingOptions.WaitFor` (e.g. wait for a selector or a JS signal) rather than guessing a
`RenderDelay`.

### Headers and footers

```csharp
renderer.RenderingOptions.TextFooter = new TextHeaderFooter {
    LeftText = "{date}", RightText = "{page} of {total-pages}",
    FontFamily = IronSoftware.Drawing.FontTypes.Helvetica,   // NOT a string
    FontSize = 9, DrawDividerLine = true
};
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter {
    HtmlFragment = "<div style='text-align:center'><img src='logo.png'></div>",
    BaseUrl = "file:///assets/", MaxHeight = 25,
    LoadStylesAndCSSFromMainHtmlDocument = true
};
```

Merge fields available in text and HTML header/footer strings: `{page}`,
`{total-pages}`, `{url}`, `{date}`, `{time}`, `{html-title}`, `{pdf-title}`.

Adding them to an **existing** PDF: `pdf.AddTextHeaders(...)`, `pdf.AddHtmlFooters(...)`,
`pdf.AddTextHeadersAndFooters(options)`, `pdf.AddHtmlHeadersAndFooters(options)`.

### Combine and rearrange pages

```csharp
PdfDocument merged = PdfDocument.Merge(new[] { pdfA, pdfB, pdfC });
pdfA.AppendPdf(pdfB);  pdfA.PrependPdf(cover);  pdfA.InsertPdf(insert, atIndex: 2);
PdfDocument part = pdf.CopyPages(0, 4);          // extract a range → new document
pdf.RemovePages(new[] { 3, 4 });
pdf.RotateAllPages(IronPdf.Rendering.PdfPageRotation.Clockwise90);
pdf.RotatePage(0, IronPdf.Rendering.PdfPageRotation.None);
pdf.CombinePages(newWidth: 595, newHeight: 842, columns: 2, rows: 2);   // n-up
int n = pdf.PageCount;
```

Page indexes are **zero-based** throughout the API.

### Stamps, watermarks, backgrounds

```csharp
pdf.ApplyWatermark("<h1 style='color:red'>DRAFT</h1>", rotation: 45, opacity: 30);

pdf.ApplyStamp(new IronPdf.Editing.TextStamper {
    Text = "PAID", FontFamily = "Arial", FontSize = 30, IsBold = true,
    VerticalAlignment = IronPdf.Editing.VerticalAlignment.Bottom,
    HorizontalAlignment = IronPdf.Editing.HorizontalAlignment.Right,
    Opacity = 50, Rotation = 15
});
// Also: ImageStamper, HtmlStamper, BarcodeStamper; ApplyMultipleStamps(...) for a batch.
// Stamper knobs: Opacity, Rotation, Scale, Hyperlink, IsStampBehindContent,
// VerticalOffset/HorizontalOffset, MinWidth/MaxWidth.

pdf.AddBackgroundPdf("letterhead.pdf");
pdf.AddForegroundOverlayPdf("overlay.pdf");
```

### Forms (AcroForms)

```csharp
var pdf = PdfDocument.FromFile("form.pdf");
foreach (var field in pdf.Form)
    Console.WriteLine($"{field.Name} = {field.Value}");

var text = pdf.Form.GetField("applicant_name");   // IronSoftware.Forms.TextFormField etc.
text.Value = "Ada Lovelace";
pdf.Flatten();                                     // make fields non-editable
pdf.SaveAs("filled.pdf");
```

Field types live in `IronSoftware.Forms` (`TextFormField`, `ComboboxFormField`,
`ImageFormField`, …). Generate a form from HTML inputs with
`RenderingOptions.CreatePdfFormsFromHtml = true`. Custom form fonts:
`pdf.Form.SetFormFont(...)` / `pdf.SetFormFontFromFile(...)`.

### Passwords, permissions, redaction

```csharp
var s = pdf.SecuritySettings;                       // IronPdf.Security.PdfSecuritySettings
s.OwnerPassword = ownerPw;                          // required to change permissions
s.UserPassword  = userPw;                           // required to open
s.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
s.AllowUserEdits    = IronPdf.Security.PdfEditSecurity.NoEdit;
s.AllowUserCopyPasteContent = false;
s.AllowUserFormData = true;
pdf.SaveAs("secure.pdf");

pdf.SecuritySettings.MakePdfDocumentReadOnly(ownerPw);
pdf.SecuritySettings.RemovePasswordsAndEncryption();

pdf.RedactTextOnAllPages("Account 12345");          // removes text, not just covers it
pdf.RedactRegionOnPage(0, new IronSoftware.Drawing.RectangleF(100, 100, 200, 50));
```

Open an encrypted file with `new PdfDocument("in.pdf", password: "user", ownerPassword: "owner")`.

### Digital signatures

```csharp
pdf.SignWithFile("cert.pfx", certPassword, timeStampUrl: "http://timestamp.digicert.com");

var sig = new IronPdf.Signing.PdfSignature("cert.pfx", certPassword) {
    SigningReason = "Approved", SigningLocation = "London", SigningContact = "ops@acme.com"
};
// Visible signature image: page index and placement rectangle are required.
sig.LoadSignatureImageFromFile("signature.png", PageIndex: 0,
    new IronSoftware.Drawing.Rectangle(100, 100, 150, 50));
pdf.Sign(sig, IronPdf.Signing.SignaturePermissions.NoChangesAllowed);
pdf.SaveAs("signed.pdf");                           // sign, then save — order matters

bool ok = PdfDocument.VerifyPdfSignaturesInFile("signed.pdf");
foreach (var v in pdf.GetVerifiedSignatures())
    Console.WriteLine($"{v.SignerName}: valid={v.Valid} status={v.Status}");
```

Also available: `SignWithStore(thumbprint, ...)` for a certificate store, `SignAndSave(path,
hsmSigner, ...)` for HSM/KMS signing, and `RemoveSignatures()`.

### Read a PDF: text, images, tables, metadata

```csharp
string all  = pdf.ExtractAllText();                 // TextExtractionOrder.LogicalOrder default
string page = pdf.ExtractTextFromPage(0);
List<IronSoftware.Drawing.AnyBitmap> imgs = pdf.ExtractAllImages();

// PdfExtractor is STATIC and works from a file path, not a PdfDocument instance.
var result = IronPdf.Extractions.PdfExtractor.Extract("statement.pdf",
    new IronPdf.Extractions.PdfExtractionOptions {
        EnableTableExtraction = true, UseFirstRowAsHeader = true });
string text = result.FullText;
foreach (var t in result.Tables)                    // IronPdf.Extractions.TableObject
    Console.WriteLine($"p{t.PageNumber} {t.RowCount}x{t.ColumnCount} {t.GetCell(0, 0)}");
// Also: result.GetTablesByPage(n), t.GetColumnHeaders(), t.GetDataAsStrings(),
// and IronPdf.Extractions.ExportManager.ExportTables(...) → CSV / JSON / XML / HTML / TXT.

pdf.MetaData.Title = "Q3 Report";                   // Author, Subject, Keywords,
pdf.MetaData.Author = "Finance";                    // CreationDate, CustomProperties
var bookmarks = pdf.Bookmarks;
var attachments = pdf.Attachments;                  // AddAttachment(name, bytes)
```

`pdf.Pages[i]` exposes `Width`, `Height`, `PageRotation`, `Text`, `Lines`, `Characters`.

### Rasterize, convert, compress

```csharp
pdf.ToPngImages("page_*.png", DPI: 150);            // also ToJpegImages, RasterizeToImageFiles
var bmp = pdf.PageToBitmap(0, DPI: 300);            // ...HighQuality variants exist
pdf.ToMultiPageTiffImage("all-pages.tiff");
pdf.SaveAsHtml("out.html");   pdf.SaveAsSvg("page_*.svg");

pdf.SaveAsPdfA("archive.pdf", IronPdf.PdfAVersions.PdfA3b);
pdf.SaveAsPdfUA("accessible.pdf");
pdf.CompressAndSaveAs("small.pdf", JpegQuality: 60);
pdf.Compress(new CompressionOptions { /* ... */ });
pdf.SaveAsLinearized("web.pdf");                    // fast web view
```

### Print

```csharp
pdf.Print();                    // default printer
pdf.PrintToFile("out.ps");
var doc = pdf.GetPrintDocument();  // full System.Drawing.Printing control
```

### OCR a scanned PDF

`pdf.PerformOcr()` works when IronOCR is also referenced. For scanned-document workflows use
the **ironocr** skill: it reads the scan and writes a searchable PDF, which you then read
back here with `ExtractAllText()`.

## Deployment

| Environment | What to do |
|---|---|
| Docker / Linux | Use `IronPdf.Linux` (or `.ARM`). Either install Chromium's native deps in the image, or set `Installation.LinuxAndDockerDependenciesAutoConfig = true` — the first run then spends 2–3 minutes on `apt-get` and needs root, so it fails in restricted containers. Baking the deps into the image is the reliable option. |
| Azure App Service | `Installation.LinuxAndDockerDependenciesAutoConfig = true`; use a **paid** tier — the free/shared tiers block the browser process. Set `Installation.TempFolderPath` to a writable path. |
| AWS Lambda | Needs a container image (the zip size limit and native deps rule out plain zips); give it ≥1 GB memory and a writable `/tmp` via `Installation.TempFolderPath`. |
| IIS | The app-pool identity needs a writable temp/deployment directory and "Load User Profile" enabled. |
| Containers, low memory | `Installation.SingleProcess = true`, `Installation.ChromeGpuMode = ChromeGpuModes.Disabled`, `Installation.ChromeBrowserLimit` to bound the pool. |
| Remote engine | `Installation.ConnectToIronPdfHost(configuration)` points a `IronPdf.Slim` client at a separate IronPdfEngine host/container. |

Other `Installation` knobs: `TempFolderPath`, `CustomDeploymentDirectory`,
`ChromeBrowserCachePath`, `EnableWebSecurity`, `SkipInitialization`, `Initialize()` (warm up
at startup so the first user request isn't slow), `CleanupTempImages()`,
`SendAnonymousAnalyticsAndCrashData`.

Performance: reuse one `ChromePdfRenderer` instance, prefer the async render methods in
servers, call `Installation.Initialize()` at boot, and `Dispose()` documents (`PdfDocument`
is `IDisposable`) in long-running processes.

## When something fails

| Symptom | Cause and fix |
|---|---|
| Watermark on every page, or `SaveAs` throws `Production use: Requires a license` | No valid licence applied. Set `IronPdf.License.LicenseKey` **before** rendering. A key for a different Iron product or product line is rejected. |
| `PlatformNotSupportedException: Dynamic code generation…` | Native AOT. Set `PublishAot=false` (or `#:property PublishAot=false` in a file-based app). |
| `IronPdfNativeException` / missing Chromium on startup | Wrong platform package. Match the table in Install; on `Slim`, set `AutomaticallyDownloadNativeBinaries`. |
| Hangs or times out in a container | Native deps missing, or no writable temp dir. Set `TempFolderPath`, install deps, try `SingleProcess = true`. |
| Blank or half-rendered page | Async content finished after the snapshot. Set `EnableJavaScript = true` and use `RenderingOptions.WaitFor`; raise `Timeout`. |
| Images/CSS missing from HTML | Relative paths have no base. Use `RenderHtmlFileAsPdf`, or pass `baseUrl` to `RenderHtmlAsPdf`. |
| Wrong fonts on Linux | The container has no fonts. Install the font packages (or embed web fonts via `CustomCssUrl`). |
| Text extraction returns nothing | The PDF is a scan — no text layer. OCR it first (see the ironocr skill). |
| `Unable to open` on an existing PDF | Encrypted. Pass `password` / `ownerPassword` to the `PdfDocument` constructor. |

## Rules

- **Never invent a member.** If unsure whether a method or property exists, check the XML
  documentation that ships inside the package before writing code:
  `grep -o 'name="[MPF]:IronPdf\.[^"]*Sign[^"]*"' ~/.nuget/packages/ironpdf.slim/<version>/lib/netstandard2.0/IronPdf.xml`
  That file is the authoritative surface for the installed version. IntelliSense in an IDE
  reads the same file.
- Do not mix this API with `IronPdf.Universal`.
- Keep licence keys out of source and out of terminal output.
- Say when output is watermarked, and never claim a document is production-ready if it is.
- Zero-based page indexes; margins in millimetres.
- Sign as the **last** step before saving — later edits invalidate a signature.
- Redaction via `RedactTextOnPage` removes content; drawing a black rectangle does not.
- Official docs and full API reference: <https://ironpdf.com/docs/>. Support:
  support@ironsoftware.com.