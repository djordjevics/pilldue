---
name: github-ticket-flow
description: Create and update GitHub issues, sub-issues, dependencies, and Project status. Use when the user asks to track work, file a bug or feature, break down a task, plan a release, update a project board, or open a pull request that should reference an issue.
---

# GitHub ticket flow

Use GitHub Issues for tracked work and one GitHub Project as the board. Requires GitHub CLI 2.94.0 or later (`gh --version`). Pass `--repo OWNER/REPO` when the current directory is not the target repository. Always pass `--title` and `--body` (or `--body-file`) so `gh` does not open an interactive prompt.

## Model

- An issue is one outcome that can ship in one pull request.
- A parent issue is a larger outcome. Sub-issues are its slices. Progress is the sub-issue count.
- `blocked by` means another issue must finish first. Use it for order, not for slices of the same outcome.
- Status lives only in the project Status field: Backlog, Ready, In Progress, Done.
- Labels name an area (`api`, `ui`, `docs`). Milestone names a release (`v0.4`). An iteration is a one-week time box.
- Priority, when the project has that field, is Now, Next, or Later.

## When to open an issue

Open an issue when the user wants the work tracked, or the change is more than a trivial fix.

Leave a rough idea as a draft item on the project. Turn it into an issue only when it can meet Ready.

If the work will take more than one pull request, create a parent and sub-issues before implementation. Each sub-issue must itself fit in one pull request.

## Definition of Ready

An issue is Ready only when all of these are true:

- The title states the outcome.
- The body has acceptance criteria a reviewer can check.
- The work fits in one pull request, or it is a parent whose sub-issues each fit in one pull request.

Otherwise leave it in Backlog.

Titles:

- Use: `Export the report as CSV`
- Avoid: `Add export` or `CSV work`

## Issue body

```markdown
## Outcome
What is true for the user when this is done.

## Acceptance criteria
- [ ] Observable result
- [ ] Edge case or failure behavior

## Notes
Constraints, links, or out of scope.
```

Omit Notes when there is nothing to say.

Set `--type` only when the repository belongs to an organization that has issue types. Use `Bug` for broken behavior, `Feature` for a new outcome, and `Task` for other tracked work. If `gh` reports that the type does not exist, create the issue again without `--type`.

## Commands

Create an issue:

```bash
gh issue create --repo OWNER/REPO --title "Export the report as CSV" --type Feature --body-file issue-body.md
```

Write `issue-body.md` first, then delete that temp file after the issue exists. On success, `gh` prints the issue URL. Use that number for later commands.

Add it to an existing project by title:

```bash
gh issue create --repo OWNER/REPO --title "Export the report as CSV" --body-file issue-body.md --project "Project title"
```

Create a slice under a parent:

```bash
gh issue create --repo OWNER/REPO --title "Stream rows instead of buffering the file" --type Task --parent 12 --body-file issue-body.md
```

Attach existing issues:

```bash
gh issue edit 12 --repo OWNER/REPO --add-sub-issue 34,35
gh issue edit 34 --repo OWNER/REPO --parent 12
```

Read structure before editing it:

```bash
gh issue view 12 --repo OWNER/REPO --json number,title,state,type,parent,subIssues,blockedBy,blocking
```

## Dependencies

Add `blocked by` when this issue cannot start until another issue is done:

```bash
gh issue edit 34 --repo OWNER/REPO --add-blocked-by 18
```

Remove a relationship that is no longer true:

```bash
gh issue edit 34 --repo OWNER/REPO --remove-blocked-by 18
```

A parent/sub-issue link and a blocked-by link answer different questions. Use both on the same pair only when the slice also cannot start yet.

## Status

Agents may move Status like this:

- New issue that is not Ready: Backlog
- Acceptance criteria written and sized to one pull request: Ready
- The user asks to start it, or implementation starts: In Progress

Keep one or two issues In Progress. If an In Progress issue no longer fits in one pull request, stop and split it into sub-issues before writing more code.

Do not set Done in the same step as opening a pull request. A merged pull request that fully finishes the issue closes it, and the project workflow sets Done.

Change a project Status field only when the user asks, or when you created the issue and the board would otherwise show the wrong column. Look up ids with `gh project field-list` and `gh project item-list`. Do not invent field ids. If those commands fail for lack of the `project` scope, run `gh auth refresh -s project` and retry once.

## Pull requests

In the pull request body:

- Use `Closes #34` when the change meets every acceptance criterion on that issue.
- Use `Refs #34` when the change is only part of the issue, and leave the issue open.

One issue should close from one pull request. A parent issue closes after its sub-issues are done, not from a pull request that lands only one slice.

## Project setup

Do this only when the user asks to set up tracking, and the repo does not already have a project.

- Create one project for the repo. If a project already tracks it, use that one.
- Status options, in order: Backlog, Ready, In Progress, Done. Add In Review only when someone else reviews pull requests.
- Turn on these built-in workflows only:
  - Auto-add items matching `is:issue` from this repository
  - Item added to project → Status Backlog
  - Item closed → Status Done
  - Pull request merged → Status Done
  - Archive Done items after they are no longer current
- Leave Backlog → Ready and Ready → In Progress as manual moves.
- Add a Priority field (Now, Next, Later) and a one-week iteration field only if the user wants scheduling.

Confirm the setup with a disposable issue: it appears as Backlog, and closing it moves it to Done. Then close or delete that issue.

## Report back

After you create or change tickets, reply with:

- Issue number, title, URL, and type
- Parent number for any sub-issue
- Blocked-by links
- Status you set, or that the board workflow will set it
- The pull request line you used (`Closes #N` or `Refs #N`) when you opened a pull request

## Boundaries

- Keep status out of labels, title prefixes, and milestone names.
- Create a milestone only for a release the user named.
- Skip story points and a second priority label.
- Skip a second project card for the pull request when the issue is already on the board.
- Leave unmet acceptance criteria open. Do not use a closing keyword for a partial fix.
