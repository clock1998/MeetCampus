# MeetCampus MVP Implementation Plan

## Purpose

This document indexes small, testable implementation increments for the University Compatibility Matching System MVP. It follows the suggested implementation order in [mvp-spec.md](mvp-spec.md) and the repository architecture in [../.copilotinstructions](../.copilotinstructions).

The MVP specification is the source of truth. Each phase must preserve its invariants: deterministic compatibility rather than popularity, finite recommendation batches, explainable results, mutual consent for messaging, privacy, and server-side enforcement.

## Delivery sequence

| Phase | Plan | Outcome |
| --- | --- | --- |
| 0 | [00-architecture-baseline.md](implementation-plans/00-architecture-baseline.md) | Confirm the hosted Blazor WebAssembly architecture and establish the test baseline. |
| 1 | [01-data-and-configuration-foundation.md](implementation-plans/01-data-and-configuration-foundation.md) | Add versioned MVP data models, validated configuration, migrations, and administrative seed/data-management boundaries. |
| 2 | [02-account-and-discoverability-eligibility.md](implementation-plans/02-account-and-discoverability-eligibility.md) | Enforce university-email verification and central discoverability eligibility. |
| 3 | [03-profile-questionnaire-and-images.md](implementation-plans/03-profile-questionnaire-and-images.md) | Deliver privacy-safe profile, image-upload, and baseline-questionnaire onboarding. |
| 4 | [04-deterministic-compatibility-scoring.md](implementation-plans/04-deterministic-compatibility-scoring.md) | Deliver pure, versioned compatibility and evidence-coverage scoring. |
| 5 | [05-candidate-screening-and-recommendation-batches.md](implementation-plans/05-candidate-screening-and-recommendation-batches.md) | Generate reproducible, finite recommendation batches. |
| 6 | [06-recommendation-decisions-and-interest.md](implementation-plans/06-recommendation-decisions-and-interest.md) | Deliver recommendation UI plus pass and interest decisions. |
| 7 | [07-mutual-interest-quotas-and-connections.md](implementation-plans/07-mutual-interest-quotas-and-connections.md) | Create transaction-safe contact transitions, quotas, and accepted connections. |
| 8 | [08-conversations-and-messaging.md](implementation-plans/08-conversations-and-messaging.md) | Deliver basic authorized text conversations. |
| 9 | [09-blocks-reports-and-moderation.md](implementation-plans/09-blocks-reports-and-moderation.md) | Deliver immediate blocks, reports, moderation actions, and audit records. |
| 10 | [10-integration-validation-and-pilot-readiness.md](implementation-plans/10-integration-validation-and-pilot-readiness.md) | Complete cross-feature security/concurrency tests and pilot release checks. |

A phase may begin only when its prerequisites are complete and its open product decisions have been resolved. Each increment should add or update focused automated tests before it is considered complete.

## Required implementation boundaries

- **Server (`MeetCampus`)**: EF Core entities, migrations, Identity integration, feature services, controllers, authorization, server-side image storage, and workers.
- **Contracts (`MeetCampus.Contracts`)**: immutable request/response records only; never EF entities or server services.
- **Client (`MeetCampus.Client`)**: Blazor WebAssembly pages/components, localized resources, and typed feature HTTP clients.
- **Data access**: PostgreSQL through `ApplicationDbContext`; schema work must use EF Core migrations and foreign keys/indexes required by the specification.
- **API safety**: server validation and authorization are mandatory; public responses must never include university emails, private answers, or object-storage credentials.

## Product decisions requiring confirmation

Do not resolve these in code until product direction is provided:

1. **Verification limits:** Verification-token lifetime and rate-limit values are unspecified and need validated defaults before implementation.
2. **Operational administration:** Questionnaire administration uses a restricted Administrator-only UI and protected API. Other operational surfaces remain scoped to their individual feature plans.

The following decisions are resolved for MVP or explicitly deferred: daily recommendations (3 new per user/day, 9 maximum unseen); dummy/no-op email delivery; age policy and image-approval workflow deferred beyond MVP; sensitive traits are not categorically excluded from required configured fields.

## Increment completion definition

Every increment must:

1. Build successfully.
2. Add or update focused automated tests for new server behavior.
3. Validate authorization, privacy, and invalid-state handling at the API/service boundary.
4. Keep user-facing client text localized.
5. Include an EF Core migration when persistent schema changes are introduced.
6. Leave no behavior that relies on client-only enforcement.
