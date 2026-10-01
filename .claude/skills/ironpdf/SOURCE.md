# Skill provenance

`SKILL.md` and `llms.txt` in this directory are vendored verbatim from Iron Software.

**Do not hand-edit them.** Refresh instead, so the copies stay diffable against
upstream. Editing them destroys the only way to detect that they have gone stale.

| File | Source | Fetched | Verified unchanged upstream |
|---|---|---|---|
| `SKILL.md` | <https://ironpdf.com/skill.md> | 2026-10-01 | 2026-10-01 |
| `llms.txt` | <https://ironpdf.com/llms.txt> | 2026-10-01 | 2026-10-01 |

This repository pins `IronPdf` **2026.9.2** in
[`src/OrderPdf.Infrastructure/OrderPdf.Infrastructure.csproj`](../../../../src/OrderPdf.Infrastructure/OrderPdf.Infrastructure.csproj).
The snapshot above was taken while that pin was current.

`llms.txt` is a generated index of ironpdf.com, not hand-written documentation.
It includes marketing and competitor-comparison pages. Claude reads `SKILL.md`;
`llms.txt` is here as a link index only.

## Refresh

```bash
curl -sS -o .claude/skills/ironpdf/SKILL.md  https://ironpdf.com/skill.md
curl -sS -o .claude/skills/ironpdf/llms.txt  https://ironpdf.com/llms.txt
git diff --stat .claude/skills/ironpdf/
```

Review the diff before committing, then update the dates and the version above.
`.gitattributes` normalises these files to LF so the diff reflects real content
changes rather than line endings.

## Snapshot log

| Date | Checked | Result |
|---|---|---|
| 2026-10-01 | Both files compared against ironpdf.com | `SKILL.md` identical. `llms.txt` 46 differing lines — doc-page title rewrites, URLs unchanged. |
