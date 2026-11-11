---
name: review-pr
description: Review a GitHub pull request against Pilldue house standards in .cursor/rules. Use when the user says @review-pr, review PR N, or pastes a PR URL for a standards review. Fetches via gh and does not post until the user selects findings.
disable-model-invocation: true
---

# Review PR (standards)

Explicit review against this repo's rules. Not Bugbot or Security Review.

## When to use

- User: `@review-pr 123`, `review PR 123`, or a PR URL for this repo
- Do not run `dotnet build` or `dotnet test` unless the user asks

## GitHub client

Prefer `gh`. Do not install a GitHub MCP server or add project MCP config for this skill.

## Phase 1 — fetch and review (read-only)

Stop after the findings table. Do not post comments.

### 1. Resolve the PR

```powershell
gh pr view <n> --json number,title,baseRefName,headRefName,headRefOid,files,url
```

Confirm the PR is in this repository. Note `baseRefName` (usually `main`) and `headRefOid`.

### 2. Diff

```powershell
gh pr diff <n>
```

Do not check out the PR branch unless the working tree is clean and a full file is required. If checkout would overwrite local changes, stop and ask.

### 3. Route rules

Read [checklist.md](checklist.md) after routing. Always also read `.cursor/rules/project-context.mdc` and `.cursor/rules/layer-structure.mdc`.

| Changed path | Also read |
|--------------|-----------|
| `src/Pilldue.Business/` | `model-separation` |
| `src/Pilldue.Data/` | `model-separation`, `migrations` |
| `src/Pilldue.UI.Wpf/`, `src/Pilldue.Business/Localization/` | `localization-both-resx`, `model-separation` |
| `src/Pilldue.UI.Desktop/` | `layer-structure` |
| `tests/` | `testing` |
| `Localization` keys in `UiLocalizer.cs` | `localization-both-resx` |

Do not load every `.mdc`. Skip generic bug hunting unless it breaks a house rule.

### 4. Findings

```markdown
## Standards review — PR #N (`head` → `base`)
**Verdict:** pass | pass with suggestions | block

| ID | Severity | Location | In diff? | Rule | Finding |
|----|----------|----------|----------|------|---------|
| 1 | Blocker | src/Pilldue.UI.Wpf/Foo.cs:42 | yes | layer-structure | ... |
```

- Severity: Blocker, Warning, or Nit
- `In diff?` is `yes` only when GitHub can attach an inline comment on that changed line
- Number findings from 1

Ask which IDs to post, if any, and which event: `COMMENT` (default), `REQUEST_CHANGES`, or `APPROVE`.

## Phase 2 — submit only after the user chooses

Do not post on `@review-pr` alone.

1. Inline comments only for rows with `In diff? = yes`. Other findings go in the review body.
2. Write JSON to a temp file and submit one review:

```powershell
gh api repos/{owner}/{repo}/pulls/{n}/reviews --method POST --input payload.json
```

```json
{
  "commit_id": "<headRefOid>",
  "event": "COMMENT",
  "body": "Standards review summary…",
  "comments": [
    { "path": "src/Pilldue.UI.Wpf/Foo.cs", "line": 42, "side": "RIGHT", "body": "…" }
  ]
}
```

Reply with the review URL. Do not change application code unless the user asks.
