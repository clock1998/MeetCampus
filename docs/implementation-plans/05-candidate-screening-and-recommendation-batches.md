# Phase 5: Candidate Screening and Recommendation Batches

## Goal
Generate finite, reproducible recommendation batches after all hard eligibility and safety conditions are met.

## Prerequisites
- Phase 4 complete.
- Use the configured MVP defaults from `mvp-spec.md`: three new recommendations per user per day, nine maximum unseen, daily refresh.

## Increments
1. Implement a candidate query/service that excludes self, inactive/ineligible accounts, unavailable recipients, blocks in either direction, moderation restrictions, incompatible intents, current suppression windows, and existing disallowed contact states.
2. Apply hard constraints before scoring; include only candidates at or above the configured minimum compatibility score.
3. Implement a background worker or scheduled service that generates batches on the approved cadence; its scheduling must not depend on a client request.
4. Enforce configured batch and unseen limits transactionally/idempotently.
5. Rank by compatibility only and apply a persisted deterministic seeded tie-breaker for equal scores.
6. Persist batch/item timestamp, viewer/candidate IDs, algorithm/questionnaire/scoring versions, compatibility, coverage, scoring reference, explanation inputs, seed, and decision state.
7. Expose a privacy-safe, viewer-scoped API to retrieve current unseen/saved recommendations only; do not implement search, pagination, or an infinite feed.

## Tests and exit criteria
- Integration tests verify every exclusion condition.
- Tests verify suppression after passes and configured unseen/batch limits.
- Fixed source data/configuration/seed reproduces the same result ordering and metadata.
- API tests verify no email/private answer/exact score data leaks.
