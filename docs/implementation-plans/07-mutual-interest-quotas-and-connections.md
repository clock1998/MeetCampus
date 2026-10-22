# Phase 7: Mutual Interest, Quotas, and Connections

## Goal
Turn compatible mutual interest into accepted connections with explicit, transaction-safe lifecycle state and capacity enforcement.

## Prerequisites
- Phase 6 complete.

## Increments
1. Implement server-side contact lifecycle transitions for `pending`, `accepted`, `declined` where applicable, `expired`, `cancelled`, and `blocked`.
2. When an eligible counterpart expresses compatible interest, atomically transition mutual pending signals to accepted, create the connection, and create/open one conversation.
3. Enforce maximum pending outbound and rolling-seven-day inbound limits inside a database transaction with concurrency protection.
4. On recipient inbound capacity, temporarily remove that recipient from eligibility; restore eligibility when the rolling count drops and all other conditions pass.
5. Implement pending expiration in a background worker with configured lifetime and idempotent processing.
6. Add cancellation/ending behavior and ensure blocks terminate relevant states immediately.

## Privacy rules
- Never disclose the reason a pending request does not become accepted.
- Messaging authorization remains denied until `accepted` and is rechecked later by Phase 8.

## Tests and exit criteria
- Transition tests reject invalid state changes.
- Concurrent integration tests prove outbound/inbound quotas cannot be exceeded.
- Tests prove capacity removal/restoration, expiry, cancellation, and blocking behavior.
- Tests prove accepted connections require both active compatible signals.
