# Phase 0: Architecture Baseline

## Goal
Confirm the current hosted Blazor WebAssembly architecture before adding MVP behavior.

## Prerequisites
None.

## Increments
1. Inventory existing `Auth` and `Profile` features, contracts, client HTTP clients, entities, migrations, configuration, and tests.
2. Document the current data model and identify fields/features that conflict with the MVP privacy requirements.
3. Establish the test-project strategy if tests are absent; use unit tests for pure rules and integration tests against PostgreSQL-compatible infrastructure for persistence/authorization flows.
4. Verify that build, database migration startup, cookie authentication, and profile-image storage configuration work in the target environment.

## Constraints
- Keep the hosted WASM architecture and the `MeetCampus` → client hosting relationship.
- Do not introduce JWT authentication or expose server persistence types to the client.
- Inventory existing profile/questionnaire fields and ensure their privacy classification and API exposure match the MVP specification.

## Tests and exit criteria
- Solution builds.
- Existing tests run, or a documented test baseline exists.
- Architecture, data ownership, and migration workflow are documented in the implementation issue/PR.
- Product-decision register from the index is reviewed before Phase 1.

## Execution results

### Architecture and data inventory
- Solution contains three projects: `MeetCampus` (ASP.NET Core host/server), `MeetCampus.Client` (hosted Blazor WebAssembly UI), and `MeetCampus.Contracts` (shared API contracts). All target `net10.0`; server uses PostgreSQL/Npgsql, EF Core, and ASP.NET Core Identity cookies.
- The server references the client for hosted WASM rendering and references contracts. The client references contracts; no EF Core persistence types are used by the client.
- Authentication includes Identity registration, login, confirmation, recovery, account management, antiforgery-protected auth operations, and serialized client authentication state. `RequireConfirmedAccount` is enabled. Email sending is registered to the no-op sender.
- The profile feature has authenticated, current-user-scoped setup and image endpoints. Profile data is in a separate profile table from Identity email data. Current profile fields include study domain, school, language, intention, ethnicity, and optional gender/sexuality; ethnicity is currently marked required by the profile contract. No public discovery-profile API exists yet.
- Profile images are validated/optimized server-side, stored with the Garage-compatible S3 service, and returned as time-limited signed URLs. The service configures path-style requests and the `garage` signing region. No image approval state or moderation workflow exists, consistent with the current MVP scope.
- `ApplicationDbContext` extends Identity's context and configures profile/lookup/image relationships. Three migrations and a model snapshot are present: initial schema, lookup seeding, and profile-image support. At the Phase 0 baseline, `Program.cs` applied migrations during startup; that behavior has since been removed in favor of the controlled workflow in `docs/database-migrations.md`. Identity data is still seeded at startup against the prepared schema.

### Privacy and Phase 1 dependencies
- Existing auth-specific responses can expose the signed-in user's own email; the profile setup response contains profile lookup values and image URLs. No public-profile, recommendation, conversation, questionnaire, block, report, or moderation APIs exist yet.
- Profile fields have no per-field privacy-class metadata. Phase 1/3 must define privacy classifications for fields and questionnaire answers, and separate private, matching-only, and shareable values before adding public-profile or recommendation contracts.
- Existing single-valued `IntentionId` is insufficient for multiple intents and directional compatibility. There is no account lifecycle/discoverability state, allowed-domain model, baseline questionnaire/version model, scoring configuration, recommendation persistence, or contact/messaging/safety schema.
- Phase 1 should add those versioned models/configuration and constraints through EF migrations. Establish unit tests for pure deterministic rules and integration tests against PostgreSQL for migrations, authorization, persistence, and concurrency. No test project or test framework is currently present.

### Validation performed and limitations
- `dotnet build MeetCampus.slnx` succeeded. It reported 12 warnings, including NU1902/NU1903 vulnerability advisories for `SixLabors.ImageSharp` 2.1.10.
- Visual Studio Test Explorer found no tests. Baseline test strategy is to add focused unit and PostgreSQL-backed integration test projects before implementing matching and persistence workflows.
- Cookie/Identity and antiforgery registration were confirmed by code inspection only; no authenticated runtime flow was exercised.
- Database startup/migration was not run during Phase 0: the checked-in connection string used placeholder credentials. Runtime migration has since been removed; use the documented deployment script and isolated test database for migration validation.
- Garage connectivity was not tested: configured S3 access credentials are blank. Keep real credentials in secrets/environment configuration, not source control.
- The checked-in application settings also contain a literal seeded administrator password. Remove it from tracked configuration and rotate it before any shared or deployed environment; do not use it as a real credential.
- The product-decision register was reviewed. Recommendation defaults, dummy email delivery, age-policy deferral, image-approval deferral, and sensitive-trait scope follow the current MVP specification. Rate-limit/token defaults and the restricted administrative surface remain open.

### Phase 0 exit status
Architecture inventory, privacy/data gap analysis, build validation, and test strategy are documented. Cookie-auth runtime behavior and Garage operations remain unverified pending safe test-environment configuration. Runtime migration is no longer part of application startup; deployment applies reviewed SQL, and integration tests require an isolated PostgreSQL test connection.
