# CLAUDE.md

This is an educational project, not a full production solution. Keep cognitive load theory
in mind and do not introduce new technologies, concepts, or libraries without checking
first. If a pattern you are about to write is not described below, stop and ask.

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

**Stack:** ASP.NET Core on .NET 10 (backend), Angular (frontend), one repository, one
PostgreSQL database. No Docker in this project; students run PostgreSQL natively.

**Current status:** backend scaffolded and building, with a functional Identity module.
The worked reference module is the next step. Frontend not started. Work on the backend
unless asked otherwise.

## Repository layout

```
backend/          .NET solution (Explorer.slnx)
frontend/         Angular workspace  (not yet created)
docs/
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
  through that module's declared public surface only.
- Shared code is owned by the platform team. Adding to the shared kernel is a decision,
  not a convenience. Resist the urge to move things there.
- Every feature module has an identical structure. Consistency beats local cleverness,
  because students learn by pattern-matching against the reference module.
- All modules are pre-scaffolded so students never edit central registration files,
  project files, or the solution. Every new dependency goes through the platform team,
  because versions are pinned centrally in a platform-owned file.

---

# BACKEND

## Shape

One deployable ASP.NET Core host. One database. One schema per module. No foreign keys
across schemas. A module refers to another module's data by ID only and resolves it
through that module's public surface, never with a join.

```
backend/
  Explorer.slnx                    solution (slnx format)                    [platform]
  Directory.Build.props            net10.0, Nullable, TreatWarningsAsErrors  [platform]
  Directory.Packages.props         central package versions                  [platform]
  Host.Api/                        composition root: program, JWT validation,
                                   exception middleware, OpenAPI + Scalar    [platform]
  Host.Tests/                      ArchUnitNET architecture tests            [platform]
  Shared/
    Shared.Domain/                 Entity, AggregateRoot, DomainException,
                                   NotFoundException                        [platform]
    Shared.Api/                    ClaimsPrincipal.GetUserId()               [platform]
    Shared.Infrastructure/         AddModuleDbContext (Npgsql, schema-per-
                                   module, per-schema migrations history)    [platform]
  Modules/
    Identity/
      Identity/                    single project, platform-owned, see below [platform]
      Identity.Tests/                                                        [platform]
    <Name>/
      <Name>.Api/                  API controllers, HTTP request/response types
      <Name>.Application/          services, DTOs, mapper profile (AutoMapper), port interfaces
      <Name>.Contracts/            inter-module interface, where modules interact through RPC
      <Name>.Domain/               entities, value objects, domain services, invariants
      <Name>.Infrastructure/       DbContext, EF config, port implementations, DI
      <Name>.Tests/                Unit/ and Integration/ folders
```

Tests live inside the module folder they test, so a team's ownership is one folder. There
is no separate tests tree; the arch tests sit in `Host.Tests` beside the host.

Five source projects per module. That is deliberate: `<Name>.Api` referencing only
`<Name>.Application` means an endpoint physically cannot name the `DbContext`, so the
usual novice shortcut of querying the database straight from an endpoint does not compile.
The compiler enforces the layering instead of a reviewer having to spot it.

## Reference rules

- `Domain` references `Shared.Domain` and nothing else. No EF Core, no ASP.NET.
- `Application` references `Domain` and its own `Contracts`. Never `Infrastructure`,
  never ASP.NET.
- `Contracts` references nothing. Primitives and IDs only.
- `Api` references `Application` and `Shared.Api`, plus
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.
- `Infrastructure` references `Application`, `Domain`, and `Shared.Infrastructure`.
- A module may reference another module's `Contracts` project and nothing else from it.
- No module references `Identity`. Other modules carry a `UserId` as a plain ID.
- `Host.Api` references every module's `Infrastructure` (for DI) and `Api` (for controller
  discovery), plus the `Identity` project.

Verified by ArchUnitNET in `Host.Tests` and run in CI. If you add a project reference,
expect the arch tests to have an opinion about it.

## Project SDKs and build

Only `Host.Api` uses `Microsoft.NET.Sdk.Web`. Every other project, including `<Name>.Api`,
is a plain `Microsoft.NET.Sdk` class library. `<Name>.Api` gets the ASP.NET types through a
`FrameworkReference`, not a NuGet package, so there is no version to drift.

Central package management: `Directory.Packages.props` pins every version; project files
carry versionless `PackageReference` lines. `Directory.Build.props` turns on nullable
reference types and treats warnings as errors, solution-wide. Both files are
platform-owned.

## Identity

Identity is a deliberate exception to the module structure: one platform-owned project
that students use but never modify or reference. It contains ASP.NET Core Identity on the
`identity` schema, users with multiple roles (`administrator`, `explorer`, seeded at
startup), and two endpoints: `POST /api/identity/register` and `POST /api/identity/login`,
both returning a JWT with `sub`, `email`, and role claims. No token persistence, no
logout, no full RBAC. Registration assigns the `explorer` role.

Token issuance lives in Identity; token validation is configured in `Host.Api`. Role
checks are `[Authorize(Roles = ...)]` attributes on controllers or actions. Controllers
read the caller's ID with `User.GetUserId()` from `Shared.Api` and pass it to application services as an
explicit `Guid userId` parameter. There is no ambient current-user abstraction.

Identity applies its EF migration and seeds roles via a hosted service at startup.
Migrations are added with `dotnet ef` (tool manifest in `.config/`), e.g.
`dotnet ef migrations add X --project Modules/Identity/Identity --startup-project Host.Api`.

## Application layer patterns

**No MediatR. No CQRS in the read-model sense.** What we keep is command/query separation
as a structuring discipline: a method either changes state and returns little, or returns
data and changes nothing. Never both.

Each module has, per aggregate:

- `<Aggregate>Service` for commands. Public sealed class, no interface. Constructor
  injects the repository and `IUnitOfWork`. A command method loads the aggregate, calls a
  method on it, and saves once.
- `<Aggregate>Queries` for reads. Public sealed class, no interface. Depends on the
  module's read port and returns DTOs.

Business rules live in the domain, not in the service. A service method that contains an
`if` about business state is a smell; move it into the aggregate.

**Interfaces.** An interface earns its place when it points a dependency arrow backwards,
or when the caller lives in a different module. Otherwise write a sealed class. Do not
create `IOrderService` next to `OrderService`; `Api` already depends on `Application`, so
there is nothing to invert and the interface hides nothing.

Interfaces that do belong, all declared in `Application` and implemented in
`Infrastructure`:

- `I<Aggregate>Repository` for the write side
- `I<Name>Queries` for the read side
- `IUnitOfWork`

Plus `I<Name>Api` in `Contracts`, implemented in `Application`.

**Repositories.** One per aggregate root, concrete and named. No `IRepository<T>`, no
`Find(Expression<Func<T, bool>>)`, no `IQueryable` crossing out of `Infrastructure`. Methods
are named after what the module actually needs and return aggregates.

**Unit of work.** `IUnitOfWork` has a single `SaveChangesAsync` method. The module's
`DbContext` implements it, which requires no code because the method is already there.
Commands call it exactly once, at the end. This is the transaction boundary.

Explain it to students in two sentences: everything changed since loading is written in one
transaction when you save, and that pattern has a name. Do not turn it into a lecture.

**Read side.** `I<Name>Queries` is implemented in `Infrastructure` using `AsNoTracking` and
projecting directly to DTOs. Read methods never load an aggregate and never mutate.

**Fat service guard.** Keep the folder structure feature-first
(`Application/Orders/PlaceOrder/`), not layer-first. If a service passes roughly seven
public methods, raise it: the team should split by use case rather than keep growing one
class.

## Validation and errors

There is no validation layer at the API. Request binding does type-shape checking and
nothing more; the domain is the sole authority on what valid means. Invariants are
enforced in aggregate constructors and methods, which throw `DomainException` (or
`NotFoundException` where a referenced thing does not exist). One platform-owned
middleware in `Host.Api` maps `DomainException` to 400, `NotFoundException` to 404, and
anything else to a logged 500, all as ProblemDetails. Services and controllers contain no
try/catch and no validation code.

## Events

There are none. Modules communicate synchronously through `I<Name>Api` in `Contracts`.
No domain events, no integration events, no in-process bus. If a real cross-module
reaction need appears mid-semester, introducing a bus is a platform-team decision at that
point, not machinery installed in advance.

## Controllers

Attribute-routed API controllers in `<Name>.Api`, one per aggregate, marked
`[ApiController]` and routed under `api/<name>/...`. Each `Api` project exposes one
`Add<Name>Controllers(this IMvcBuilder)` extension that registers its assembly as an
application part; `Host.Api` calls `AddControllers()` once and chains these. Controllers
inject the application service or queries class directly. An action does exactly three
things: bind the request, call one application method, map the result to an HTTP response.

Actions return `ActionResult<T>`, never bare `IActionResult`: the OpenAPI document is
generated from the action signatures, and the frontend's `api-types.ts` is generated from
that document, so an untyped action starves the frontend of types.

HTTP request and response types live here, not in `Application` and never in `Contracts`.
They are shaped for the wire. Do not return domain entities.

## Visibility

`Contracts` and `Api` are the two public surfaces of a module, aimed at different audiences:
`Contracts` faces other modules, `Api` faces the outside world. Keep them separate. An
HTTP DTO never belongs in `Contracts`.

`Infrastructure` types are `internal` except the `AddXxxModule` extension method. The
`DbContext`, repositories, query implementations, and EF configurations are all internal,
with `InternalsVisibleTo` for the module's test project (declared in the `.csproj`).
Domain and Application types are public because sibling projects in the same module need
them.

## Testing

xUnit with FluentAssertions, pinned to FluentAssertions 7 (the last Apache-licensed
line; do not bump to 8 without a licensing discussion). One test project per module with
`Unit/` and `Integration/` folders.

Integration tests send real HTTP requests: `WebApplicationFactory<Program>` boots the
host, tests call endpoints with an `HttpClient`. They run against a local PostgreSQL
database (`explorer-test` by default, overridable with the `EXPLORER_TEST_DATABASE`
environment variable, which CI could use). Test projects are exempt from the module
reference rules; they may reference `Host.Api`. `Identity.Tests` is the live example of
the pattern.

Architecture tests in `Host.Tests` use ArchUnitNET and encode the reference rules above.

## Conventions

- Each module exposes one `AddXxxModule(IServiceCollection, IConfiguration)` extension in
  `Infrastructure`, and one `Add<Name>Controllers(IMvcBuilder)` extension in `Api`.
  `Host.Api` calls those two. Nothing else.
- Each module has its own `DbContext` mapped to its own schema, registered through
  `AddModuleDbContext` from `Shared.Infrastructure`, with its own migrations history
  table inside that schema.
- Inter-module reads go through `I<Name>Api` in `Contracts`, implemented in `Application`.
- Adding to a `Contracts` project is a cross-team negotiation. Flag it rather than quietly
  extending it.
- Local development configuration (connection string with `postgres`/`admin`, dev JWT
  key) lives in `appsettings.Development.json`. Production values are deliberately blank
  in `appsettings.json`.
- The OpenAPI document is at `/openapi/v1.json`; Scalar's browsable UI is at `/scalar`
  in development.

---

# FRONTEND

Not started. Build only when asked. When it starts, the guiding constraint is that
students are frontend novices, so favour code they can read over code that is clever.

Do not mirror the backend layering here. The frontend has no invariants to protect and no
persistence to abstract, so the equivalents are the smart/dumb component split and the
`public-api.ts` rule. That is the whole architecture.

## Shape

Single `ng new` workspace. One application. No Nx, no libraries, no `NgModule`.
Standalone components only.

```
frontend/src/app/
  core/                      auth, http interceptors, layout, app.routes.ts   [platform]
  shared/
    ui/                      dumb components used by everyone                 [platform]
    util/
    api-types.ts             generated from OpenAPI, do not hand-edit         [platform]
  modules/
    <name>/
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

## Conventions

- State lives in a service holding signals, injected with `inject()`. No NgRx, no
  facades, no RxJS beyond `HttpClient` and `toSignal`.
- DTO types are generated from the backend OpenAPI document into `shared/api-types.ts`.
  Students write their own small `HttpClient` services but never hand-write a DTO.
- UI comes from the chosen component library, wrapped in `shared/ui` where a default
  configuration is needed. Do not hand-roll tables, dialogs, or form controls.
- One fully implemented reference module exists as the pattern to copy: list with
  filtering, detail, create form with validation, error and loading states, tests.
  Match it.

---

## Still open, ask before choosing

- **Reference module.** Which module carries the fully worked example (aggregate,
  service, queries, repository, endpoints, tests), and what the sample aggregate is.
  This depends on the application's domain, which is not yet written down here.
- **AutoMapper version.** The pattern is decided (mapper profile in `Application`), the
  package is not yet added. AutoMapper changed to a commercial license in 2025; pick the
  version deliberately when the reference module needs it, as was done for
  FluentAssertions.
- **Mocking library.** Deferred until the reference module has something to mock.
- **Test database reset strategy.** Currently tests share `explorer-test` and rely on
  unique data per run; decide on a reset approach when the first module tests land.
- **Frontend component library.** Not chosen.
- **CODEOWNERS and per-module AGENTS.md.** Deliberately not created yet; add when teams
  and the domain are fixed.

## Working style

- Prefer the smallest change that satisfies the request. This is teaching code, so
  readability outranks sophistication.
- When a change would touch a platform-owned file, a `Contracts` surface, or a `.csproj`,
  say so before doing it.
- When a decision is genuinely open, ask rather than guess.
- Prefer a plain class over an abstraction. If you are adding an interface, be able to say
  which dependency arrow it reverses.
