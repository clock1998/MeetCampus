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
