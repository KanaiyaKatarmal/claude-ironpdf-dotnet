# Contributing

Thanks for your interest. This is a community project built with IronPDF, not an
official Iron Software sample.

## Before you start

```bash
dotnet restore
dotnet build
dotnet test
```

`dotnet test` renders real PDFs through Chromium. The first run downloads roughly
100 MB of native binaries and can take several minutes. Subsequent runs are fast.

Set a license key if you have one, otherwise the generated PDFs carry a trial
watermark:

```bash
# Windows PowerShell
$env:IRONPDF_LICENSE_KEY="YOUR_KEY_HERE"

# Linux / macOS
export IRONPDF_LICENSE_KEY="YOUR_KEY_HERE"
```

**Never commit a license key.** Not in `appsettings.json`, not in a test, not in a
commit message.

## Ground rules

- **Do not edit `.claude/skills/ironpdf/SKILL.md` or `llms.txt`.** They are vendored
  verbatim from ironpdf.com so they can be diffed against upstream. To update them,
  follow the refresh steps in [`.claude/skills/ironpdf/SOURCE.md`](.claude/skills/ironpdf/SOURCE.md)
  and update the dates in that file in the same commit.
- **Do not duplicate the skill into `CLAUDE.md`.** A root copy existed once; it
  doubled the context and went stale.
- Everything interpolated into the PDF template goes through `WebUtility.HtmlEncode`.
- Money stays `decimal`.
- `OrderPdf.Domain` takes no dependency on any framework or on IronPDF.

## Changing the PDF output

Page-break CSS in `TemplateRenderer.cs` is load-bearing for the 65-item sample order.
If you touch layout, regenerate and eyeball all three sample PDFs:

```bash
dotnet test --filter GenerateAndSaveAllSamplePdfsToOutputDirectory
```

Then check `output/sample-large-order.pdf` specifically for repeating table headers,
no split rows, and correct `Page n of m` numbering. Commit regenerated PDFs
deliberately, in their own commit, and say which IronPDF version produced them.

## Pull requests

- One topic per PR.
- Say which platform you tested on. The repo references the Windows `IronPdf`
  package by default; see [`docs/deployment.md`](docs/deployment.md) for others.
- If your change touches IronPDF API usage, licensing, or deployment, tag
  `@iron-software` for review.
- If you are reporting a rendering bug, attach the generated PDF and the IronPDF
  version from its Producer metadata.

## Attribution

`CONTRIBUTORS.md` records who did what. Add yourself in the same PR as your first
substantive change, and describe the work rather than claiming the project.
