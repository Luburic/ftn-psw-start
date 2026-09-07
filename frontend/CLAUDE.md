# Frontend

Mandatory patterns for `frontend/`. The root `CLAUDE.md` holds the project context and
the rules that apply to both tiers; this file adds the frontend-specific ones. If a
pattern you are about to write is not described here, stop and ask.

Scaffolded: shell, auth in `core/auth`, and initial Exploration (tours) and Social
(blogs) modules; Games and Payment are placeholders. The guiding constraint: students are frontend
novices who know HTML, CSS, JavaScript and the basics of React, and nothing about
Angular. Favour code they can read over code that is clever. Do not mirror the backend
layering — there are no invariants to protect and no persistence to abstract. The
architecture is the smart/dumb component split plus the `public-api.ts` rule, and
nothing more.

## Shape

Single `ng new` workspace on the current Angular major (v22 at the time of writing).
One application. No Nx, no libraries, no `NgModule`, no component library. Standalone
components only, zoneless, `OnPush` by default (both are the `ng new` defaults; do not
opt out of either).

```
frontend/src/
  styles.scss                @use of the four partials below, nothing else   [platform]
  styles/
    _tokens.scss             colours, spacing, radius, type as CSS custom properties
    _base.scss               reset, body, headings, links
    _layout.scss             page container, stack, grid helpers
    _components.scss         global classes: button, card, form field, table
  app/
    core/                    auth state, http interceptors, layout, app.routes.ts [platform]
    shared/
      util/
      api/                   shared envelope types (PageResult, ProblemDetails),
                             generated, do not hand-edit                        [platform]
    modules/
      <name>/
        api/                 this module's generated DTO types, do not hand-edit
        pages/               routed (smart) components, one folder per component
          tour-list/
            tour-list.ts
            tour-list.html
            tour-list.scss
        components/          presentational (dumb) components, used only inside this
                             module, one folder per component
        services/            state service, api calls
        models/
        <name>.routes.ts
        public-api.ts        the only file other modules may import
```

The `pages/`, `components/`, `services/` folders are a deliberate divergence from the
2025 Angular style guide, which groups by feature. Here the folder names make the
smart/dumb split physical, which is the one architecture idea the frontend teaches.
Inside those folders the layout is exactly what `ng generate component` produces: a
folder per component holding `tour-list.ts`, `tour-list.html`, `tour-list.scss`, no
`.component` suffix. Generate with `ng g c pages/tour-list` from the module folder.

## Rules

- A module may import from its own folder, from `shared/`, from `core/auth` (the
  current user), and from another module's `public-api.ts`. Nothing else.
- `public-api.ts` stays thin. A growing public API is a design smell worth raising.
- Enforced by `no-restricted-imports` in `eslint.config.js` (`angular-eslint`, flat
  config), one block per module whose regex names the sibling modules and exempts
  their `public-api`. This is a lint rule, not a compiler guarantee. Unlike the
  backend, nothing structurally prevents a violation.
- Routes are lazy-loaded per module with `loadChildren` from `core/app.routes.ts`. That
  file is platform-owned and set up once.
- Cross-module composition, in order of preference: navigate to the other module's
  route; embed a component it exports from `public-api.ts` (IDs in, outputs out, injects
  its own module's services internally); never share state. `public-api.ts` exports
  components, routes, and types — never services. Cross-module data composition happens
  in the backend through `Contracts`, never in the frontend.
- The one legitimately shared state is the current user (identity, roles) in `core/`.
  Modules read it; only the platform team writes it.

## The Angular subset

Students learn exactly this subset and the code uses nothing outside it. Adding a
construct means adding a lesson, so treat it as a platform decision.

- Components: standalone, `inject()` for dependencies (never constructor injection),
  `input()` / `output()` signals (never decorators), `host` property (never
  `@HostBinding`), `protected` members read by the template, `readonly` on
  framework-initialised properties.
- Templates: built-in control flow (`@if`, `@for`, `@switch`), `class` and `style`
  bindings. No `*ngIf`, `*ngFor`, `NgClass`, `NgStyle`.
- State: `signal()` and `computed()`. `effect()` only when a signal must
  drive something outside the component tree, and it needs a reason. The one
  instance is the blog detail page passing its route input to the service's resource. State that outlives
  a page lives in a service holding signals, `providedIn: 'root'`.
- HTTP: `httpResource()` for reads (it carries loading and error state as signals);
  `HttpClient` with `firstValueFrom` for writes. Functional interceptors
  (`HttpInterceptorFn`, `withInterceptors`) in `core/`.
- Forms: Signal Forms (`form()`, `FormField`, validators from
  `@angular/forms/signals`). No Reactive Forms, no template-driven forms.
- Routing: `router-outlet`, `routerLink`, `loadChildren`, route parameters bound as
  signal inputs via `withComponentInputBinding`; `inject(Router)` only for navigation
  after a command.
- No RxJS in student code. `Observable` appears only as the return type of `HttpClient`
  writes, immediately awaited. No NgRx, no facades.

## Styling

No component library. Styling is plain SCSS, deliberately minimal: elegant defaults in
lean files that students extend, not a design system. The four global partials in
`src/styles/` are platform-owned and short. Theming means editing `_tokens.scss` and
nothing else; every colour, spacing and font value elsewhere refers to a custom property
defined there.

Where a style goes, in order:

1. A token value changes: `_tokens.scss`.
2. A class used by more than one module: `_components.scss` or `_layout.scss`. Adding a
   global class is shared code and follows the same promotion rule as `shared/`.
3. Everything else: the component's own `.scss`, under default (emulated) encapsulation.
   Never `::ng-deep`.

Do not hand-roll a design system, do not add utility-class frameworks, do not introduce
`shared/ui` wrappers unprompted. Visual consistency across modules is an open problem
assigned to the platform team, and misalignment is an accepted learning experience.

## Conventions

- DTO types are generated per module from the backend OpenAPI document (split by module
  tag) into `modules/<name>/api/`, shared envelope types into `shared/api/`, so the
  import boundary covers types as well. The platform team owns the generation script.
  Students never hand-write a DTO.
- A page component owns the interaction: it injects the module's service, reads its
  signals, and passes plain values down to presentational components, which take
  `input()`s and raise `output()`s and inject nothing.
- Do not produce Vitest tests unless specifically instructed. Frontend testing is not
  taught; it is a platform-team assignment.
- One fully implemented reference module exists as the pattern to copy: list with
  filtering, detail, create form with validation, error and loading states. Match it.

## Still open, ask before choosing

- **Reference frontend module.** Follows the backend reference module decision.
- **Type generation tool.** The per-module split is decided, the tool that produces it
  is not. Until then the files in `modules/<name>/api/` and `shared/api/` are
  hand-written stand-ins shaped exactly as a generator would emit them.
