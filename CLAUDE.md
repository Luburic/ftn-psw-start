# CLAUDE.md

This is a temporary document which will be used to guide the development of the starting
project. In the end we will come back to it to update it.

This is an educational project, not a full production solution. Keep cognitive load theory
in mind and do not introduce new technologies, concepts, or libraries without checking with
me first. If a pattern you are about to write is not described below, stop and ask.

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

**Stack:** ASP.NET Core (backend), Angular (frontend), one repository, one database.

**Current status:** backend under construction. Frontend not started. Work on the
backend unless asked otherwise.

## Repository layout

```
backend/          .NET solution
frontend/         Angular workspace  (not yet created)
docs/
.github/
CODEOWNERS
```

Module paths are strictly parallel across the two tiers, so a team's ownership is two
globs:

```
backend/src/Modules/<Name>/
frontend/src/app/modules/<name>/
```

## Rules that apply everywhere

- One module never reaches into another module's internals. Cross-module access goes
  through that module's declared public surface only.
- Shared code is owned by the platform team. Adding to the shared kernel is a decision,
  not a convenience. Resist the urge to move things there.
- Every module has an identical structure. Consistency beats local cleverness, because
  students learn by pattern-matching against the reference module.
- All modules are pre-scaffolded before the semester so students never edit central
  registration files, project files, or the solution.
- Each module has its own `AGENTS.md` stating its scope. Stay inside it.

---

# BACKEND

## Shape

One deployable ASP.NET Core host. One database. One schema per module. No foreign keys
across schemas. A module refers to another module's data by ID only and resolves it
through that module's public surface, never with a join.

```
backend/
  src/
    Host.Api/                        composition root, program, middleware    [platform]
    Shared/
      Shared.Domain/                 Entity, AggregateRoot                    [platform] (for VOs we use C# records, we do not use domain events at the start)
      Shared.Infrastructure/         EF base config, auth, in-process bus     [platform]
    Modules/
      <Name>/
        <Name>.Api/                  minimal API endpoints, HTTP request/response types
        <Name>.Application/          services, DTOs, mapper profile (AutoMapper), port interfaces
        <Name>.Contracts/            inter-module interface, where modules interact through RPC
        <Name>.Domain/               entities, value objects, domain services, invariants
        <Name>.Infrastructure/       DbContext, EF config, port implementations, DI
  tests/
    ArchitectureTests/                                                        [platform]
    Modules/<Name>.Tests/
```

Five projects per module. That is deliberate: `<Name>.Api` referencing only
`<Name>.Application` means an endpoint physically cannot name the `DbContext`, so the
usual novice shortcut of querying the database straight from an endpoint does not compile.
The compiler enforces the layering instead of a reviewer having to spot it.

## Reference rules

- `Domain` references `Shared.Domain` and nothing else. No EF Core, no ASP.NET.
- `Application` references `Domain` and its own `Contracts`. Never `Infrastructure`,
  never ASP.NET.
- `Contracts` references nothing. Primitives and IDs only.
- `Api` references `Application` only, plus `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.
- `Infrastructure` references `Application`, `Domain`, and `Shared.Infrastructure`.
- A module may reference another module's `Contracts` project and nothing else from it.
- `Host.Api` references every module's `Infrastructure` (for DI) and `Api` (for endpoint
  mapping).

Verified by ArchUnitNET in `tests/ArchitectureTests` and run in CI. If you add a project
reference, expect the arch tests to have an opinion about it.

## Project SDKs

Only `Host.Api` uses `Microsoft.NET.Sdk.Web`. Every other project, including `<Name>.Api`,
is a plain `Microsoft.NET.Sdk` class library. `<Name>.Api` gets the ASP.NET types through a
`FrameworkReference`, not a NuGet package, so there is no version to drift.

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
- cross-cutting ports such as `ICurrentUser` and `IClock` (platform-owned, in `Shared`)

Plus `I<Name>Api` in `Contracts`, implemented in `Application`.

**Repositories.** One per aggregate root, concrete and named. No `IRepository<T>`, no
`Find(Expression<Func<T, bool>>)`, no `IQueryable` crossing out of `Infrastructure`. Methods
are named after what the module actually needs and return aggregates.

**Unit of work.** `IUnitOfWork` has a single `SaveChangesAsync` method. The module's
`DbContext` implements it, which requires no code because the method is already there.
Commands call it exactly once, at the end. This is the transaction boundary and it is also
where domain events are dispatched, via an override of `SaveChangesAsync` that collects
events off tracked aggregates and publishes them after the write.

Explain it to students in two sentences: everything changed since loading is written in one
transaction when you save, and that pattern has a name. Do not turn it into a lecture.

**Read side.** `I<Name>Queries` is implemented in `Infrastructure` using `AsNoTracking` and
projecting directly to DTOs. Read methods never load an aggregate and never mutate.

**Fat service guard.** Keep the folder structure feature-first
(`Application/Orders/PlaceOrder/`), not layer-first. If a service passes roughly seven
public methods, raise it: the team should split by use case rather than keep growing one
class.

## Endpoints

Minimal APIs in `<Name>.Api`, grouped by aggregate, mapped through one
`Map<Name>Endpoints(this IEndpointRouteBuilder)` extension method. Endpoints inject the
application service or queries class directly. They do exactly three things: bind the
request, call one application method, map the result to an HTTP response.

HTTP request and response types live here, not in `Application` and never in `Contracts`.
They are shaped for the wire. Do not return domain entities.

## Visibility

`Contracts` and `Api` are the two public surfaces of a module, aimed at different audiences:
`Contracts` faces other modules, `Api` faces the outside world. Keep them separate. An
integration event never belongs in `Api`; an HTTP DTO never belongs in `Contracts`.

`Infrastructure` types are `internal` except the `AddXxxModule` extension method. The
`DbContext`, repositories, query implementations, and EF configurations are all internal,
with `[assembly: InternalsVisibleTo("<Name>.Tests")]` for the module's test project. Domain
and Application types are public because sibling projects in the same module need them.

## Conventions

- Each module exposes one `AddXxxModule(IServiceCollection, IConfiguration)` extension in
  `Infrastructure`. `Host.Api` calls that plus `Map<Name>Endpoints()`. Nothing else.
- Each module has its own `DbContext` mapped to its own schema, with its own migrations
  history table.
- Domain events stay inside a module and may use module types. Integration events cross
  the boundary and carry only primitives and IDs. Keep the two clearly separate.
- Inter-module reads go through `I<Name>Api` in `Contracts`, implemented in `Application`.
  State changes are announced with integration events on the in-process bus.
- Adding to a `Contracts` project is a cross-team negotiation. Flag it rather than quietly
  extending it.

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

These are not yet decided. Do not pick one silently; every choice here gets copied five
times by five different teams.

- **Validation.** Where request validation lives and whether it uses data annotations, an
  endpoint filter, or a library. A library is a new dependency, so it needs a decision.
- **In-process event bus.** Hand-rolled dispatcher versus an existing library.
- **Domain event dispatch timing.** Before or after `SaveChangesAsync`, and whether an
  outbox is in scope at all.
- **Read side placement.** `I<Name>Queries` as a separate port, as written above, versus
  folding read methods onto the repository. Currently separate.
- **Auth.** Scheme, and how `ICurrentUser` is populated.
- **Frontend component library.** Not chosen.
- **Module names and count.** Placeholders only so far.

## Working style

- Prefer the smallest change that satisfies the request. This is teaching code, so
  readability outranks sophistication.
- When a change would touch a platform-owned file, a `Contracts` surface, or a `.csproj`,
  say so before doing it.
- When a decision is genuinely open, ask rather than guess.
- Prefer a plain class over an abstraction. If you are adding an interface, be able to say
  which dependency arrow it reverses.
