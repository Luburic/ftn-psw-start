# Frontend

Mandatory patterns for `frontend/`. The root `CLAUDE.md` holds the project context and
the rules that apply to both tiers; this file adds the frontend-specific ones. If a
pattern you are about to write is not described here, stop and ask.

Not started; build only when asked. The guiding constraint: students are frontend
novices, so favour code they can read over code that is clever. Do not mirror the
backend layering — there are no invariants to protect and no persistence to abstract.
The architecture is the smart/dumb component split plus the `public-api.ts` rule, and
nothing more.

## Shape

Single `ng new` workspace. One application. No Nx, no libraries, no `NgModule`.
Standalone components only.

```
frontend/src/app/
  core/                      auth, http interceptors, layout, app.routes.ts   [platform]
  shared/
    util/
    api/                     shared envelope types (PageResult, ProblemDetails),
                             generated, do not hand-edit                      [platform]
  modules/
    <name>/
      api/                   this module's generated DTO types, do not hand-edit
      pages/                 routed components
      components/            presentational, used only inside this module
      services/              api service, state
      models/
      <name>.routes.ts
      public-api.ts          the only file other modules may import
```

## Rules

- A module may import from its own folder, from `shared/`, and from another module's
  `public-api.ts`. Nothing else.
- `public-api.ts` stays thin. A growing public API is a design smell worth raising.
- Enforced by `no-restricted-imports` in `eslint.config.js`, one identical block per
  module. This is a lint rule, not a compiler guarantee. Unlike the backend, nothing
  structurally prevents a violation.
- Routes are lazy-loaded per module from `core/app.routes.ts`. That file is
  platform-owned and set up once.
- Cross-module composition, in order of preference: navigate to the other module's
  route; embed a component it exports from `public-api.ts` (IDs in, outputs out, injects
  its own module's services internally); never share state. `public-api.ts` exports
  components, routes, and types — never services. Cross-module data composition happens
  in the backend through `Contracts`, never in the frontend.

## Conventions

- State lives in a service holding signals, injected with `inject()`. No NgRx, no
  facades, no RxJS beyond `HttpClient` and `toSignal`.
- DTO types are generated per module from the backend OpenAPI document (split by module
  tag) into `modules/<name>/api/`, shared envelope types into `shared/api/`, so the
  import boundary covers types as well. The platform team owns the generation script.
  Students never hand-write a DTO.
- UI comes from the chosen component library, used directly. There is deliberately no
  shared UI wrapper layer: visual consistency across modules is an open problem assigned
  to the platform team, and misalignment is an accepted learning experience. Do not
  hand-roll tables, dialogs, or form controls, and do not introduce `shared/ui` wrappers
  unprompted.
- One fully implemented reference module exists as the pattern to copy: list with
  filtering, detail, create form with validation, error and loading states, tests.
  Match it.

## Still open, ask before choosing

- **Component library.** Not chosen.
- **Reference frontend module.** Follows the backend reference module decision.
