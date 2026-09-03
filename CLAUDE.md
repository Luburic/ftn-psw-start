# CLAUDE.md

This is an educational project, not a full production solution. Keep cognitive load theory
in mind and do not introduce new technologies, concepts, or libraries without checking
first. If a pattern you are about to write is not described in these instructions, stop
and ask.

## Context

Teaching repository for a fourth-year software engineering course. A single team of
10 to 15 students builds one modular monolith web application. Students are split into
sub-teams of 2 to 4. Each sub-team owns one feature module. One sub-team owns the
platform: shared code, build, CI, and the agent tooling.

Students know Clean Architecture and .NET. The modular monolith is the new concept for
them. They are weak on frontend.

This shapes every decision here: the backend is rigorous and enforced by tooling, the
frontend is deliberately plain and enforced by convention. That asymmetry is intentional,
not an oversight. Do not "improve" the frontend by adding architectural layers.

**Domain:** a tourism-exploration platform. Exploration holds guided tours built by
authors as sequences of key points, each carrying a segment of a narrative. Games holds
map-based challenges (a quiz, find-the-hidden-location). Social holds the community:
clubs, blogs, leaderboards. Payment monetizes the platform through a product abstraction
that can sell tours, games, bundles, or subscriptions. Cross-module synergies (a key
point that requires completing a game, a purchase that unlocks a tour) are a deliberate
learning outcome; the module that owns the consequence asks through the other module's
contract — the module that owns the cause never pushes.

**Stack:** ASP.NET Core on .NET 10 (backend), Angular (frontend), one repository, one
PostgreSQL database. No Docker in this project; students run PostgreSQL natively.

**Current status:** backend scaffolded and building with a functional Identity module;
Exploration (Tours) and Social (Blogs) carry initial feature implementations, Games and
Payment are empty scaffolds. No module is blessed as the worked reference yet. Frontend
not started; build it only when asked. Work on the backend unless asked otherwise.

## Repository layout

```
backend/          .NET solution (Explorer.slnx), see backend/CLAUDE.md
frontend/         Angular workspace (not yet created), see frontend/CLAUDE.md
docs/             knowledge base (course material, in Serbian), see below
.github/          CI workflow: restore, build, test (with a Postgres service container)
.config/          dotnet tool manifest (dotnet-ef)
```

Feature modules are Exploration, Games, Social, and Payment. Module paths are strictly
parallel across the two tiers, so a team's ownership is two globs:

```
backend/Modules/<Name>/
frontend/src/app/modules/<name>/
```

## Rules that apply everywhere

- One module never reaches into another module's internals. Cross-module access goes
  through that module's declared public surface only, and extending such a surface
  (`Contracts`, `public-api.ts`) is a cross-team negotiation — flag it rather than
  quietly extending it.
- Shared code is owned by the platform team. Adding to the shared kernel is a decision,
  not a convenience. Resist the urge to move things there.
- Every feature module has an identical structure. Consistency beats local cleverness,
  because students learn by pattern-matching against the reference module.
- All modules are pre-scaffolded so students never edit central registration files,
  project files, or the solution. Every new dependency goes through the platform team,
  because versions are pinned centrally in a platform-owned file.

## Where to look next

The instructions are scoped by tier. `backend/CLAUDE.md` and `frontend/CLAUDE.md` hold
the mandatory patterns for their folder and load automatically once a session touches
files there; read the relevant one before writing code, not after the first file is
open. READMEs placed next to the code they govern hold the finer conventions (for
example `backend/Shared/Shared.Tests/README.md` for tests). When a platform change
alters a mandatory pattern, updating the affected instructions file or README is part
of that change.

- Backend feature or fix: `backend/CLAUDE.md`.
- Frontend feature or fix: `frontend/CLAUDE.md`.
- A feature spanning both tiers: both. The seam is the backend's Application DTO, which
  is the wire contract the frontend's generated types are produced from.
- Understanding a concept or trade-off: the knowledge base, below. Do not load the
  tier instructions for a discussion that changes no code.

## Knowledge base

`docs/knowledge-base/` holds the course material students learn from, written in
Serbian and owned by the platform team. Every document is a lesson: concept teaching
whose examples are deliberately simplified and do NOT follow this project's
conventions. Expect the knowledge base and the real code to differ; that divergence
is by design, not an error to fix. Never copy code from the knowledge base into the
project.

Consult the knowledge base only when the session is about understanding: a student
asking to learn, discuss, or examine a concept or trade-off. Start from
`docs/knowledge-base/INDEX.md`, which lists every document with a one-line
description and its prerequisites, and load only the documents relevant to the
discussion. Do not read `docs/knowledge-base/mapa.html`; it is a human-facing view
of the same material.

For engineering tasks (writing or changing code), do not load knowledge-base
documents; the mandatory patterns live in the tier instructions and READMEs.

## Still open, ask before choosing

Each tier's instructions list its own open decisions. Cross-cutting:

- **CODEOWNERS and per-module instructions.** Deliberately not created yet; add when
  teams are fixed, as `CLAUDE.md` files inside each module folder so they load only for
  that module's sessions.

## Working style

- Prefer the smallest change that satisfies the request. This is teaching code, so
  readability outranks sophistication.
- When a change would touch a platform-owned file, a `Contracts` surface, or a `.csproj`,
  say so before doing it.
- When a decision is genuinely open, ask rather than guess.
- Prefer a plain class over an abstraction. If you are adding an interface, be able to say
  which dependency arrow it reverses.
