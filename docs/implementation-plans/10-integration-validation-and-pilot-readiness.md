# Phase 10: Integration Validation and Pilot Readiness

## Goal
Verify the complete MVP against the specification before a limited university pilot.

## Prerequisites
- Phases 0–9 complete.
- All product decisions in the implementation-plan index resolved and recorded in validated configuration or approved policy.

## Increments
1. Build a traceability matrix mapping every acceptance criterion in specification section 10 to automated tests and manual pilot checks.
2. Add end-to-end/integration coverage for identity, public-contract privacy, discoverability changes, blocks, recommendation generation, interest/connection transitions, messaging authorization, reporting, and moderator actions.
3. Add concurrent quota tests against the selected relational database setup; verify no over-allocation under parallel interest attempts.
4. Run reproducibility tests using stored versions/seeds and verify explanations are grounded and private-answer-safe.
5. Validate all configuration in a deployment-like environment, including Garage S3 compatibility, database migrations, dummy/no-op email delivery, rate limiting, authorization roles, and worker scheduling.
6. Prepare operational runbooks for moderation, account restrictions, migration/rollback, configuration changes, worker failures, and incident response. Track production email, age policy, and image-approval workflow as deferred follow-up work.
7. Conduct an accessibility/privacy/security review of the Blazor client and server APIs; remove out-of-scope discovery/engagement functionality.
8. Define a limited-pilot checklist, monitoring signals that do not violate product invariants, rollback conditions, and a post-pilot review process.

## Final acceptance gate
All items in section 10 of `mvp-spec.md` must be demonstrably satisfied, including tests for authorization, privacy, matching rules, state transitions, blocks, and quota concurrency. No pilot release proceeds while any safety, privacy, or deterministic-matching invariant is unverified.
