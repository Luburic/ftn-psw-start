# Backend

Mandatory patterns for `backend/`. The root `CLAUDE.md` holds the project context and
the rules that apply to both tiers; this file adds the backend-specific ones. If a
pattern you are about to write is not described here, stop and ask.

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
    Shared.Domain/                 Entity, AggregateRoot, PageResult,
                                   DomainException, NotFoundException       [platform]
    Shared.Api/                    ClaimsPrincipal.GetUserId()               [platform]
    Shared.Infrastructure/         AddModuleDbContext (Npgsql, schema-per-
                                   module, per-schema migrations history)    [platform]
    Shared.Tests/                  ExplorerApiFactory (per-assembly test DB,
                                   reseed, test JWTs), WellKnownUsers        [platform]
  Modules/
    Identity/
      Identity/                    single project, platform-owned, see below [platform]
      Identity.Tests/                                                        [platform]
    <Name>/
      <Name>.Api/                  API controllers, speaking Application DTOs
      <Name>.Application/          services, DTOs, mapper profile (AutoMapper), port interfaces
      <Name>.Contracts/            inter-module interface, where modules interact through RPC
      <Name>.Domain/               entities, value objects, domain services, invariants
      <Name>.Infrastructure/       Persistence/ (DbContext, EF config, repositories,
                                   migrations, module initializer), DI. Future non-persistence
                                   adapters (e.g. an external API client) get sibling folders
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
that students use but never modify or reference. It holds ASP.NET Core Identity on the
`identity` schema, register/login endpoints issuing JWTs, and role seeding at startup
(`administrator`, `explorer`); the details live in the project itself.

What matters outside it: token validation is configured in `Host.Api`; role checks are
`[Authorize(Roles = ...)]` attributes on controllers or actions. Controllers read the
caller's ID with `User.GetUserId()` from `Shared.Api` and pass it to application
services as an explicit `Guid userId` parameter. There is no ambient current-user
abstraction.

Migrations are added with `dotnet ef` (tool manifest in `.config/`), e.g.
`dotnet ef migrations add X --project Modules/Identity/Identity --startup-project Host.Api`.

## Application layer patterns

**No MediatR. No CQRS in the read-model sense.** What we keep is command/query separation
as a structuring discipline: a method either changes state and returns little, or returns
data and changes nothing. Never both.

The application layer is organized by use case, not as a mirror of the domain. Below the
application layer the code is shaped by data: aggregates own the domain, the persistence,
and the test seeds. From the application layer outward it is shaped by use cases:
application classes, controllers, and integration tests.

Related use cases form a group with its own folder and use-case-named classes:

- `<UseCaseGroup>Service` for commands. Public sealed class, no interface. Constructor
  injects `I<Aggregate>Repository` and `IUnitOfWork`. A command method loads the
  aggregate, calls a method on it, and saves once.
- `<UseCaseGroup>Queries` for reads. Public sealed class, no interface. Constructor
  injects `I<Aggregate>ReadRepository`, plus `I<Aggregate>Repository` when a read needs a
  domain-computed value, plus other modules' contracts when it composes data across
  modules. Returns DTOs, never mutates, never saves (an architecture test forbids a
  `*Queries` class from depending on `IUnitOfWork`).

In Exploration the groups are `TourAuthoring` and `TourBrowsing`. Group names often
start with the aggregate name because use cases cluster around aggregates, but a group
spanning aggregates is named after the use case alone. Controllers only ever inject
these service and queries classes; command/query separation lives one level down, in
the two repository interfaces.

**DTO placement.** A DTO is born in the use-case folder that accepts or returns it and
moves to the aggregate-named folder (`Application/Tours/`, which also holds the two
repository interfaces) only when a second group needs it — the same promotion rule as
for `Shared`, one level down.

Business rules live in the domain, not in the service. A service method that contains an
`if` about business state is a smell; move it into the aggregate.

**Interfaces.** An interface earns its place when it points a dependency arrow backwards,
or when the caller lives in a different module. Otherwise write a sealed class. Do not
create `IOrderService` next to `OrderService`; `Api` already depends on `Application`, so
there is nothing to invert and the interface hides nothing.

Interfaces that do belong, all declared in `Application` and implemented in
`Infrastructure`:

- `I<Aggregate>Repository` for the write side, returning aggregates
- `I<Aggregate>ReadRepository` for the read side, returning DTOs
- `IUnitOfWork`

Plus `I<Name>Api` in `Contracts`, implemented in `Application`.

**Repositories.** One per aggregate root, concrete and named. No `IRepository<T>`, no
`Find(Expression<Func<T, bool>>)`, no `IQueryable` crossing out of `Infrastructure`. Methods
are named after what the module actually needs and return aggregates.

**Unit of work.** `IUnitOfWork` has a single `SaveChangesAsync` method. The module's
`DbContext` implements it, which requires no code because the method is already there.
Commands call it exactly once, at the end. This is the transaction boundary.

**Read side.** `I<Aggregate>ReadRepository` is implemented in `Infrastructure` using
`AsNoTracking` and projecting directly to DTOs. Aggregates may also expose side-effect-free
query methods that return values derived from their own state; commands use them as guards,
and a read use case may reuse them instead of duplicating the derivation in SQL.

Deciding how to answer a request, in order:

1. Does it change state? Command: the service loads the aggregate, calls a method on it,
   saves once.
2. Is it a list or view of things to display? Query: project through the read repository.
   Never load aggregates to display them.
3. Is it a derived value one aggregate can answer from its own state? Query: load the
   aggregate through `I<Aggregate>Repository` inside the queries class, call its query
   method, wrap the value in a DTO, and do not save.

The hard rule behind all three: queries never mutate and never call `SaveChangesAsync`.
Do not write a `CanX()` next to every command speculatively; the command throwing is the
check, and the query method appears only when a real read use case asks for it.

A list query whose result can grow unbounded returns `PageResult<T>` from `Shared.Domain`
and takes `page` and `pageSize`, clamped in the queries class; a naturally small list
(an author's own tours) may stay unpaged.

**Fat service guard.** If a service passes roughly seven public methods, raise it: the
use-case group has grown too broad and should split into smaller groups.

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

A contract may expose commands as well as queries. A cross-module command runs in the
callee's own transaction, so there is no atomicity across modules. This is a known and
deliberately accepted trade-off: it keeps inter-module calls on the RPC model students
already know, and the consistency gap is planned course material mid-semester, alongside
distributed transactions. Do not introduce compensating machinery for it.

## Controllers

Attribute-routed API controllers in `<Name>.Api`, one per use-case group, marked
`[ApiController]` and routed under `api/<name>/...`. Controllers serving the same
aggregate share its route prefix (`TourAuthoringController` and `TourBrowsingController`
both route under `api/exploration/tours`) and must never declare the same verb and
route twice. Each `Api` project exposes one
`Add<Name>Controllers(this IMvcBuilder)` extension that registers its assembly as an
application part; `Host.Api` calls `AddControllers()` once and chains these. Controllers
inject the application service or the queries class directly. An action does exactly three
things: bind the request, call one application method, map the result to an HTTP response.

Actions return `ActionResult<T>`, never bare `IActionResult`: the OpenAPI document is
generated from the action signatures, and the frontend's per-module DTO types are
generated from that document, so an untyped action starves the frontend of types.

Controllers bind and return the module's Application DTOs directly; there is no second
set of request/response types in `Api`. The Application DTO is therefore the wire
contract, and the frontend's generated types change when it changes. An Api-local record
is the exception, introduced only when the wire shape genuinely diverges from the
application shape, and it needs a reason. DTOs never move to `Contracts`, and actions
never return domain entities (the arch tests enforce the latter).

## Visibility

`Contracts` faces other modules, `Api` faces the outside world; keep the two public
surfaces separate. A module's own DTOs never belong in `Contracts` — contract types are
separate, negotiated, and deliberately minimal.

`Infrastructure` types are `internal` except the `AddXxxModule` extension method. The
`DbContext`, repositories, query implementations, and EF configurations are all internal,
with `InternalsVisibleTo` for the module's test project (declared in the `.csproj`).
Domain and Application types are public because sibling projects in the same module need
them.

## Testing

xUnit with FluentAssertions, pinned to FluentAssertions 7 (the last Apache-licensed
line; do not bump to 8 without a licensing discussion). One test project per module with
`Unit/` and `Integration/` folders. Integration tests send real HTTP requests:
`WebApplicationFactory<Program>` boots the host, tests call endpoints with an
`HttpClient`. Test projects are exempt from the module reference rules; they may
reference `Host.Api`, `Shared.Tests`, and other modules' test projects (for seeds).

The core discipline is the three-channel rule: state goes in through seeds, actions go
through HTTP, observation goes through a read-only context — each concern has exactly
one channel. The full conventions (test databases, seed construction, assertion
patterns, wiring, auth) live in `Shared/Shared.Tests/README.md` and are mandatory when
writing tests. `Identity.Tests` and the Exploration and Social test projects are the
live examples.

Architecture tests in `Host.Tests` use ArchUnitNET and encode the reference rules above.

## Conventions

- Each module wires itself through exactly two extensions, both called by `Host.Api` and
  nothing else: `AddXxxModule(IServiceCollection, IConfiguration)` in `Infrastructure`
  (which registers the module's `DbContext` on its own schema via `AddModuleDbContext`)
  and `Add<Name>Controllers(IMvcBuilder)` in `Api`.
- Aggregates generate their ID in the constructor (`base(Guid.NewGuid())`), never via EF
  value generation; seed data and tests depend on IDs existing before save.
- Local development configuration (connection string with `postgres`/`admin`, dev JWT
  key) lives in `appsettings.Development.json`. Production values are deliberately blank
  in `appsettings.json`.
- The OpenAPI document is at `/openapi/v1.json`; Scalar's browsable UI is at `/scalar`
  in development.
- No C# primary constructors. Dependencies are assigned to `readonly` fields in an
  explicitly written constructor, because constructor injection in that form is the
  prior knowledge students arrive with.
- No `CancellationToken` parameters in our own code, and tokens are not forwarded to
  framework calls. They appear only where a framework interface forces them into a
  signature (e.g. `IHostedService`), where they are accepted and ignored.

## Still open, ask before choosing

- **Reference module.** Which module is blessed as the fully worked example students
  copy. Exploration and Social both carry initial implementations; neither is blessed
  yet.
- **AutoMapper version.** The pattern is decided (mapper profile in `Application`), the
  package is not yet added. AutoMapper changed to a commercial license in 2025; pick the
  version deliberately when the reference module needs it, as was done for
  FluentAssertions.
- **Mocking library.** Deferred until the reference module has something to mock.
