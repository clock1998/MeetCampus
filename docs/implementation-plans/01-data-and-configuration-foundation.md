# Phase 1: Data and Configuration Foundation

## Goal
Create the versioned, relational persistence and validated configuration foundation required by all later MVP features.

## Prerequisites
- Phase 0 complete.
- Approved administrative management surface.

## Increments
1. Add strongly typed, validated options for matching, contact, privacy, and safety values from specification section 12; reject invalid weights, ranges, and contradictory settings on startup.
2. Extend the account lifecycle model with active, paused, suspended, deactivated/removal, verification, and discoverability opt-in state without returning private email data.
3. Add database models and migrations for allowed email domains, connection intents, intent compatibility rules, questionnaire versions/questions/options, answer versions, and scoring/explanation metadata.
4. Add models/migrations for recommendation batches/items, contact/connection states, conversations/messages, blocks, reports, moderator actions, and audit events with required foreign keys, uniqueness constraints, and indexes.
5. Define restricted server-side administrative data-management services for allowlists, intent rules, questionnaire versions, and limits. Add authorization policies/roles and auditing.

## Data rules
- Use immutable contracts in `MeetCampus.Contracts`; EF entities stay server-only.
- Normalize and uniquely index email through Identity/account storage.
- Enforce unique directional blocks and one active contact state per sender/recipient pair.
- Add indexes for eligibility, batch membership, pending contact states, and conversation message order.
- Store configured/questionnaire/scoring versions with the data needed for reproducibility.

## Tests and exit criteria
- Options validation tests cover invalid weights, totals, quotas, and time periods.
- Migration applies to an empty database and retains Identity schema compatibility.
- Entity configuration tests or integration tests verify key uniqueness, foreign keys, and indexes.
- Restricted administrative operations authorize correctly and produce audit records.
