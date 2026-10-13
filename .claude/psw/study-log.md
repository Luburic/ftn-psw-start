# Study log

Shared format of the files in `.workspace/study/`. `/psw-pitaj` and `/psw-ucenje` write
them; `/psw-ucenje` reads them to decide what to ask next. Create the folder and a file
when it is missing.

A **node** is one lesson file of the knowledge base, named by its path relative to
`docs/knowledge-base/`, for example
`server/2-arhitektura-modula/1-domenski-sloj/4-agregat.md`. The `README.md` files of the
segments are introductions and are never nodes.

## `profile.json`

The git identities whose commits count as the student's own work.

```json
{ "identities": [{ "name": "Ana Anić", "email": "ana@uns.ac.rs" }] }
```

## `log.jsonl`

One JSON object per line, appended and never rewritten. Dates are `YYYY-MM-DD`; paths
are relative to the repository root.

A question the student asked through `/psw-pitaj`:

```json
{"kind":"ask","date":"2026-10-20","nodes":["klijent/1-angular/4-signali.md"],"file":"frontend/src/app/modules/social/blog-reading/blog-list/blog-list.ts","topic":"zašto je filter signal"}
```

A question `/psw-ucenje` put to the student:

```json
{"kind":"study","date":"2026-10-27","node":"server/2-arhitektura-modula/1-domenski-sloj/4-agregat.md","file":"backend/Modules/Exploration/Exploration.Domain/Tours/Tour.cs","lines":"41-63","type":"objasni","outcome":"after_push"}
```

- `type` is `objasni` or `izmeni`.
- `outcome` is `correct` (right at once), `after_push` (right after the one push), or
  `revealed` (the answer had to be given).
- `nodes` in an `ask` entry may be empty when no lesson covers the question.

## `plan-<YYYY-MM-DD-HHmm>.md`

The `Plan učenja` block that closes a round of `/psw-ucenje`, saved exactly as printed.
