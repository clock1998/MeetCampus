# Copilot Instructions
# Meet Campus - Copilot Instructions

## Project Overview

Meet Campus is a .NET 10 hosted Blazor WebAssembly application for a social/campus community experience. It uses a vertical-slice architecture, ASP.NET Core Identity with cookie authentication, Entity Framework Core with PostgreSQL, Tailwind CSS, DaisyUI, and localized client UI.

## Solution Architecture

The solution is split into three projects with explicit boundaries:

- `MeetCampus` — ASP.NET Core server host. Owns the Blazor host shell, ASP.NET Core Identity, authentication cookies, EF Core, PostgreSQL access, migrations, database seeding, API controllers, server-side feature services, and Identity protocol endpoints.
- `MeetCampus.Client` — Blazor WebAssembly client. Owns all visible application UI, routes, layouts, navigation, authentication pages, localized resources, client authentication state, browser-facing feature behavior, and typed API clients in `Features/[Feature]/HttpClients/` with shared client HTTP infrastructure in `Infrastructure/`.
- `MeetCampus.Contracts` — shared cross-boundary contracts. Contains request and response records used by server APIs and the client. Contracts are organized under `Features/[Feature]/` namespaces and folders. Do not place EF entities or server services here.

Project dependency direction:

```text
MeetCampus.Client -> MeetCampus.Contracts
MeetCampus -> MeetCampus.Client
MeetCampus -> MeetCampus.Contracts
```

The server references the client project to host the WebAssembly UI, and references contracts for API boundaries. The client references contracts and owns its typed HTTP clients; there is no separate `MeetCampus.HttpClients` project in the solution. Keep server implementation details out of `MeetCampus.Client` and `MeetCampus.Contracts`. Keep browser UI out of the server feature services and controllers.

## Feature-First Organization

Organize code by feature rather than by global technical layers.

- Client feature UI belongs under `MeetCampus.Client/Features/[Feature]/`.
  - `Pages/` contains routable `.razor` pages.
  - `Components/` contains reusable feature components when needed.
  - `Resources/` contains the feature resource marker and `.resx` files.
- Server feature implementation belongs under `MeetCampus/Features/[Feature]/`.
  - `Controllers/` contains API controllers.
  - `Services/` contains server-side application and domain orchestration.
  - `Infrastructure/` contains server-specific protocol and framework integration.
  - `Models/` contains server-only models that are not API contracts.
- Shared API contracts belong under `MeetCampus.Contracts/Features/[Feature]/` and use the matching `MeetCampus.Contracts.Features.[Feature]` namespace.
- Typed client interfaces and implementations belong under `MeetCampus.Client/Features/[Feature]/HttpClients/` and use the matching `MeetCampus.Client.Features.[Feature].HttpClients` namespace.
- Shared client HTTP infrastructure belongs under `MeetCampus.Client/Infrastructure/` and uses `MeetCampus.Client.Infrastructure`.
- Shared client layout and navigation belong under `MeetCampus.Client/Layout/` and `MeetCampus.Client/Features/Shared/`.
- Each class, interface, record, DTO/contract, and Razor component must have its own file.

Do not recreate the old server-owned Identity UI. Authentication pages are client-owned; the server retains Identity services, cookie handling, APIs, and protocol infrastructure required by external login, passkeys, and account operations.

## C# Standards

- Target `net10.0`; do not change the target framework, SDK, or language version unless explicitly requested.
- Use nullable reference types and implicit usings consistently with the project files.
- Use file-scoped namespaces and modern C# syntax supported by the target framework.
- Prefer records for immutable API contracts and DTO-like data.
- Use `required` for mandatory object properties when appropriate.
- Use PascalCase for public types, methods, and properties; use `_camelCase` for private fields.
- Use `Async` suffix for asynchronous methods.
- Use the least visible accessibility that satisfies the design; do not default to `public`.
- Do not add abstractions unless they isolate an external dependency, represent a meaningful boundary, or improve testability.
- Reuse existing services and contracts instead of wrapping them unnecessarily.
- Keep user-facing strings in localized resource files, not inline in application logic.
- Do not edit generated files, migrations, snapshots, or files marked auto-generated unless the task specifically requires it.
- Comments should explain why, not restate what the code does.

## Async, Validation, and Error Handling

- Use async/await end-to-end for I/O and database operations.
- Accept and propagate `CancellationToken ct = default` for service and data-access operations where appropriate.
- Pass cancellation tokens to EF Core and HTTP calls; do not use sync-over-async.
- Use `ArgumentNullException.ThrowIfNull` for reference arguments and validate strings with `string.IsNullOrWhiteSpace` where appropriate.
- Throw precise exceptions and preserve useful context. Do not catch or swallow `Exception` broadly.
- Do not create fire-and-forget work from request or component code.
- Validate all input on the server even when the client validates it first.
- Prefer `AsNoTracking()` for read-only EF Core queries.
- Use structured logging through `ILogger<T>` for important server operations; never log secrets, passwords, tokens, or sensitive personal data.

## Server, Data, and API Boundaries

- `MeetCampus/Data/` contains `ApplicationDbContext`, Identity/application data types, profile entities, and migrations.
- EF entities are persistence models only. Never expose them directly from controllers, services, or contracts.
- Map entities to contracts before returning API responses.
- Keep database access in server services or the data layer; client projects must not reference EF Core or `ApplicationDbContext`.
- Use PostgreSQL through the configured Npgsql provider and create migrations for schema changes.
- Apply authorization at the API boundary with `[Authorize]` or equivalent policy checks for protected operations.
- Group controllers and endpoints by feature under `MeetCampus/Features/[Feature]/Controllers/`.
- Use contracts from `MeetCampus.Contracts` for all API input and output. Do not duplicate request/response shapes in the client or server.
- Register server services, controllers, authentication, authorization, and localization in `MeetCampus/Program.cs`.
- Preserve `MeetCampus/Components/App.razor`, `MeetCampus/Components/Pages/Error.razor`, and the `MapRazorComponents<App>()` host configuration unless the hosting architecture is intentionally changed. The server still hosts the Blazor shell even though application UI belongs to the client project.

## Object Storage

- Use Garage as the S3-compatible object store for profile image uploads and reads; keep object-storage operations in server-side feature services, never in the WebAssembly client.
- Use the existing `S3ProfileImageStorage` service and configured `S3Options` rather than creating ad hoc S3 clients in controllers or components.
- Configure the AWS .NET S3 client for Garage's endpoint and path-style requests (`ForcePathStyle = true`) with the Garage signing region (`AuthenticationRegion = "garage"`); keep endpoint and credentials in configuration/secrets, not source control.
- For `PutObjectRequest`, disable chunk encoding when required for Garage signature compatibility. Request-level SDK settings belong on the request, not `AmazonS3Config`; verify options against the installed AWSSDK.S3 version before using them.
- Preserve server-side image validation/optimization and ownership checks before storing or returning profile images. Do not expose storage credentials or log signed URLs.

## Authentication and Authorization

Authentication uses ASP.NET Core Identity with application cookies, not JWT or bearer-token authentication.

- The server owns `ApplicationUser`, Identity managers, cookie configuration, account services, authentication APIs, antiforgery protection, and Identity protocol endpoints.
- The client owns login, registration, password recovery, email confirmation, account-management pages, and navigation UI.
- Use `CascadingAuthenticationState`, `AuthorizeView`, `AuthorizeRouteView`, and the existing client authentication state provider.
- Protect client routes with route authorization and redirect anonymous users through the existing `RedirectToLogin` flow.
- Use `IAuthClient` and `AuthClient` for client authentication operations; do not call Identity managers from client components.
- Use `IProfileSetupClient` and other typed clients for feature API calls; do not construct ad hoc HTTP requests in components.
- Preserve the authentication-state serialization/deserialization setup between the server host and WebAssembly client.
- Keep cookie-based authentication and antiforgery requirements consistent when adding or changing endpoints.
- Check authorization and ownership server-side for every protected resource. Client route protection is not a security boundary.
- Do not expose passwords, reset tokens, recovery codes, or other secrets in logs or API responses beyond the intended operation.

## Blazor WebAssembly Client

- Put interactive pages and components in `MeetCampus.Client` under the relevant feature folder.
- Use the existing hosted WebAssembly render-mode setup. Do not add `@rendermode InteractiveWebAssembly` blindly; follow the render mode already used by the surrounding route and host.
- Use `Routes.razor` as the client routing entry point and preserve its authorization behavior.
- Use `MainLayout.razor`, shared navigation, and reusable components instead of duplicating layout markup.
- Inject typed clients and focused services through DI. Components should coordinate UI state, not contain server or persistence logic.
- Dispose subscriptions, timers, and SignalR connections correctly when components own them.
- Prefer C# and Blazor APIs. Use JavaScript only when browser functionality cannot reasonably be implemented in .NET; isolate unavoidable interop behind a focused service.
- Use `NavigationManager` for application navigation and preserve return URLs safely when redirecting.
- Avoid blocking calls, synchronous HTTP calls, and unbounded work in lifecycle methods.

## Typed HTTP Clients

- Define a client interface only when it represents a real API boundary or is needed for testing; pair it with one implementation.
- Place feature client interfaces and implementations in `MeetCampus.Client/Features/[Feature]/HttpClients/`."
- Register clients in `MeetCampus.Client/Program.cs` with `AddHttpClient<Interface, Implementation>()` and the hosted application base address.
- Put common request, response, error, and status handling in `MeetCampus.Client/Infrastructure/` rather than duplicating it in each feature client.
- Deserialize into contracts from `MeetCampus.Contracts`; never deserialize API responses into server entities.
- Pass cancellation tokens through HTTP operations and handle expected API failures without hiding unexpected failures.

## Localization

- Add a resource marker and resource files for each feature that has user-facing text: `[Feature]Resource.cs`, `[Feature]Resource.resx`, and supported culture variants such as `[Feature]Resource.fr.resx`.
- Place client resources under `MeetCampus.Client/Features/[Feature]/Resources/`.
- Inject the feature-specific `IStringLocalizer<[Feature]Resource>` in Razor components.
- Use shared resources only for truly global strings. Do not put feature-specific keys in a shared resource.
- Keep validation, status, error, and navigation text localizable.

## Styling

- Use Tailwind CSS and DaisyUI with the existing Soft Campus design language.
- Prefer DaisyUI components for standard controls and Tailwind utilities for layout and custom styling.
- Use mobile-first responsive design and generous spacing.
- Preserve accessible labels, focus states, semantic HTML, keyboard navigation, and sufficient contrast.
- Keep custom styles compatible with the existing theme; avoid introducing unrelated UI frameworks or global styles.

## Future Real-Time Features

When implementing chat or notifications, use SignalR with server hubs under the relevant feature and a client-side wrapper under the matching client feature. Authenticate hubs, validate membership and authorization server-side, manage group membership explicitly, and dispose/reconnect client connections correctly. Do not add SignalR infrastructure to unrelated features.

## Security

- Require HTTPS in development and production configurations.
- Validate and authorize every server operation; never trust client route guards or client-provided identity data.
- Protect user privacy and return only the minimum profile data needed by the requesting operation.
- Check blocking, connection, and ownership rules before exposing profiles, sending messages, or changing relationships.
- Use ASP.NET Core Identity password hashing and token providers; never implement custom password storage.
- Use antiforgery protection for cookie-authenticated state-changing requests as required by the endpoint style.
- Store local secrets in user secrets or environment configuration, never in source control.
- Add rate limiting and abuse controls to sensitive operations such as login, password reset, connection requests, messaging, and profile access.
- Do not introduce end-to-end encryption, moderation, age verification, or other product security features without defining the required data and operational design first.

## Testing and Validation

- Use the test framework already present in the solution; if no test project exists, add a separate `[ProjectName].Tests` project rather than placing tests in production projects.
- Test public services, API behavior, authentication flows, validation, and important component behavior through public APIs.
- Follow Arrange-Act-Assert, one behavior per test, deterministic tests, and the repository's existing naming conventions.
- Avoid mocking code owned by the solution; mock external dependencies only when necessary.
- After changes, build the affected project and then the solution. Run relevant tests when available.
- For database changes, verify migrations and seeding behavior without committing secrets or local database state.
- For MeetCampus integration tests, use Testcontainers rather than a remote test database or temporary remote schema.

## Development Workflow

Before implementing a new feature, create a focused implementation plan and present it for user review. Do not implement multiple distinct features in one change. Wait for approval of the plan before making feature code changes.

1. Identify the owning feature and project boundary before editing.
2. Define or update a shared contract when an API shape changes.
3. Implement server authorization, validation, service logic, and API behavior.
4. Implement or update the typed HTTP client.
5. Implement client pages/components and localized resources.
6. Register required services and clients in the appropriate `Program.cs`.
7. Apply Tailwind/DaisyUI styling and accessibility checks.
8. Build the affected project and solution; run relevant tests.
9. Review the diff for accidental entity exposure, duplicated contracts, secrets, missing authorization, and obsolete server UI references.

## Database Migrations

- Do not create, modify, or remove EF Core migration files unless the user explicitly requests migration work. The user manages migrations.
- When a feature requires a schema change, describe the required model/schema impact in the plan and implementation summary; do not generate a migration.

## Important Project Files

- `MeetCampus/Program.cs` — server DI, middleware, Identity, database, host shell, and endpoint mapping.
- `MeetCampus/Components/App.razor` — server-hosted Blazor application shell.
- `MeetCampus.Client/Program.cs` — WebAssembly DI, authentication-state deserialization, localization, and typed client registration.
- `MeetCampus.Client/Routes.razor` — client routing and authorization route handling.
- `MeetCampus.Contracts/Features/` — shared API contracts.
- `MeetCampus.Client/Features/[Feature]/HttpClients/` — typed feature HTTP clients.
- `MeetCampus.Client/Infrastructure/` — shared client API infrastructure and exceptions.
- `MeetCampus/Data/` — EF Core context, Identity/application data, and migrations.
- `MeetCampus/Properties/launchSettings.json` — server launch profiles and browser startup behavior.

## Troubleshooting

- For authentication redirect problems, inspect `Routes.razor`, `RedirectToLogin.razor`, the client authentication state provider, server cookie configuration, and the selected server launch profile.
- For API failures, inspect the browser request, controller route, authorization policy, contract shape, typed client, and server logs in that order.
- For database failures, check the configured connection string, pending migrations, PostgreSQL availability, and startup seeding.
- For routing or render-mode problems, inspect the server `Components/App.razor`, `MapRazorComponents<App>()`, client `Routes.razor`, and additional client assemblies.
- For localization problems, verify the feature resource marker, `.resx` naming, culture initialization in `MeetCampus.Client/Program.cs`, and the injected localizer type.

## Package Vulnerability Management

- For explicitly acknowledged and accepted package vulnerability risk, suppress the corresponding NuGet advisory warning at the specific package reference rather than globally; keep other advisory warnings visible.

## Questionnaire Management

- Questionnaire definitions and answers are not versioned; do not design or implement questionnaire versioning or historical-answer preservation.

---

**Last updated:** .NET 10 hosted Blazor WebAssembly architecture with client-owned UI and typed HTTP clients, server-owned Identity/API infrastructure, and shared feature contracts.

## Project Guidelines
- For MeetCampus database and integration-test work, use the functioning remote PostgreSQL service rather than requiring local database setup; keep automated tests isolated from the app's default/shared database.