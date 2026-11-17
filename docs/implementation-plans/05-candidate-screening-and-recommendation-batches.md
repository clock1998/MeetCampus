# Phase 5: Candidate Screening and Recommendation Batches

## Goal
Generate a small daily set of recommendations for eligible members, then provide a safe API for retrieving it. Safety and account eligibility take priority over compatibility.

## Current state
- Phase 4 provides account and pair eligibility, compatibility evaluation, scoring, and generic category explanations. These are not yet connected to a recommendation generator or list API.
- All questionnaire questions are required. Missing, invalid, or outdated answers prevent scoring; an unavailable score is not a match.
- `RecommendationBatch` and `RecommendationItem` exist, including a unique viewer/date index and selected score, coverage, decision, and ordered explanation-category references.
- The recommendation API currently only reports availability. There is no batch worker, list endpoint, or decision flow yet.
- Phase 4 rejected scoring versions and evaluation history. Follow that later decision rather than older conflicting versioning requirements in `mvp-spec.md`.
- The user manages EF migrations. Verify the existing model's migration status before testing; do not generate or edit migration files unless explicitly requested.

## MVP decisions
- Use existing configuration: by default, up to **3 new recommendations per viewer per UTC day**, **9 unseen** at most, minimum compatibility score **30**, and **90 days** of pass suppression. Read settings rather than hardcoding defaults.
- Keep unseen recommendations until decided or removed because they become ineligible. For this plan, saved items do not count as unseen.
- Sort by unrounded compatibility score descending, then candidate ID ascending in a documented consistent order. Do not use a seed or popularity and activity signals.
- Exclude a candidate when **any contact request exists in either direction**, regardless of state. This includes pending, accepted, expired, cancelled, and blocked states and can prevent future rematching indefinitely. Independently enforce blocks in both directions.
- Use an in-process background worker. A successful empty batch counts as the viewer's one attempt for that UTC day. After restart or a missed run, process the current UTC day if it has no completed batch; do not backfill previous days.
- Permanently remove an unseen item from the unseen set when its candidate becomes ineligible or the pair fails a safety or contact check. If the viewer becomes ineligible, hide their recommendations instead of deleting all items. Hide unsafe saved items; Phase 6 owns their decision behavior.
- Return minimal card data and generic match explanations. Search, pagination, an infinite feed, scoring history, and decision UI are outside this phase.

## Feature 1: Screen candidates

**Outcome:** Only pairs passing hard restrictions are evaluated for scoring.

1. Add a server-side recommendations service under `MeetCampus/Features/Recommendations/Services/`. Start with a simple read-only query that excludes the viewer, contact history, existing unseen or saved items, and passed items still in their suppression period.
2. Check the viewer with `AccountEligibilityService`. Skip an ineligible viewer, including one with incomplete or outdated answers. Do not create a batch until the viewer is eligible for that run.
3. Reuse existing pair eligibility and evaluation services for account, moderation, bidirectional block, intent, and answer rules. Unknown intent rules remain a configuration error, not permission to match.
4. Exclude an existing `Passed` item while `DecidedAt + PassedProfileSuppressionDays` is in the future. If a passed item has no decision time, fail closed for that candidate and log a non-sensitive warning. Phase 6 owns writing pass decisions and timestamps.
5. Exclude candidates with a contact request in either direction in any state; also check blocks independently.
6. Skip ineligible, incomplete-answer, and unavailable-score results. Do not disclose candidate-specific reasons or prompt the viewer to update another member's answers.
7. Include only completed evaluations at or above `MinimumCompatibilityScore`. Keep exact scores, category scores, answers, and detailed candidate eligibility failures on the server.

## Feature 2: Create and store a daily batch

**Outcome:** A completed run respects the daily and unseen limits, including concurrent runs.

1. Generate a batch per eligible viewer and UTC date. Count existing unseen items across batches after cleaning up newly invalid unseen items. Add at most the smaller of the daily limit and remaining unseen capacity; if capacity is zero, save an empty completed batch.
2. Recheck pair eligibility, blocks, contact history, suppression, and current answers immediately before persistence. Recheck again when serving a recommendation or processing a later decision, since state can change afterward.
3. Sort eligible results by score descending and candidate ID ascending. Persist the chosen order explicitly if the existing model cannot preserve it. Do not promise to reproduce old results after answers, categories, or scoring code change.
4. Persist the batch timestamp, UTC date, viewer ID, and each selected candidate ID, score, coverage, unseen decision, and ordered qualifying category IDs. Do not persist individual answers, every evaluated pair, category scores, rule versions, or a seed. Resolve category names from current records; omit deleted categories.
5. Use a short PostgreSQL-coordinated per-viewer transaction when counting unseen items and writing the batch. Batch creation and unseen cleanup must use the same coordination. Keep the unique viewer/date constraint as the final idempotency guard and handle competing inserts. Score outside the transaction; revalidate selected candidates and capacity before committing.
6. Save a batch even when no candidates qualify, so a successful run is not repeated that day. A failed or cancelled run must not leave a completed partial batch and may be retried.

## Feature 3: Run the daily worker

**Outcome:** Eligible members receive batches without opening the application.

1. Register an in-process hosted service in `MeetCampus/Program.cs`. Create a DI scope for each processing unit; do not capture scoped EF services in the singleton worker.
2. On startup and after each UTC day begins, process eligible viewers without a completed batch for that UTC date. Process sequentially or with a small fixed bound rather than starting an unbounded task per member.
3. Pass cancellation tokens to delays, database calls, and loops. Stop cleanly on shutdown. Log counts and failures without answers, exact scores, signed image URLs, or sensitive profile data.
4. Treat an empty completed batch as an attempt. Retry failures without duplicating successful batches; the transaction and unique index must remain correct with multiple application instances.

## Feature 4: Retrieve current recommendations safely

**Outcome:** An authenticated viewer retrieves only their own finite unseen and saved recommendations.

1. Add response contracts under `MeetCampus.Contracts/Features/Recommendations/` and an authorized endpoint in `RecommendationsController`. Resolve the viewer from the authenticated identity; never accept a viewer ID from the client or return database entities.
2. Recheck the viewer's eligibility. If ineligible, return a safe availability result without candidate information and retain existing items. The existing own-account eligibility endpoint can explain the viewer's own required actions.
3. Recheck current pair eligibility, blocks, intent, and contact history before returning each item. Remove invalid unseen items from the unseen set permanently and hide invalid saved items. Do not reveal whether the reason was a block, incomplete answers, moderation, or another restriction. Repeat safety checks in future profile, image, and decision endpoints.
4. Return the item ID, minimum approved card fields, qualitative match label, coverage, and generic category names. Derive the label from the stored score using Phase 4 thresholds. Never return the score, private answers, detailed failure flags, storage keys, or credentials. Decide exact card fields before defining the contract; current profile models have no dedicated public display-name field.
5. If cards include an image, authorize image access for the viewer and current pair. Do not reuse an owner-only endpoint or issue long-lived public URLs. Reuse the existing server-side Garage storage service.
6. Keep the API finite and viewer-scoped. Phase 6 owns the typed WASM recommendation client, recommendation page, and pass/save/interest operations.

## Tests and exit criteria
- Unit tests cover score threshold boundaries, candidate-ID tie ordering, empty results, suppression boundaries, and all contact states in both directions.
- PostgreSQL Testcontainers integration tests cover viewer/candidate eligibility, missing and outdated answers, bidirectional blocks, intent mismatch, moderation, recipient capacity, contact history, existing unseen/saved items, pass suppression, and score threshold.
- Test daily and unseen limits, saved-item exclusion from the unseen count, empty batches, UTC rollover, restart catch-up, retries after failures, and two concurrent workers. Verify a failed transaction cannot leave a partial batch.
- Test that stale or blocked unseen items are removed from the unseen set, unsafe saved items are hidden, and an ineligible viewer does not lose all items. Test rechecks before persistence and retrieval.
- API tests verify authentication, viewer ownership, safe empty/ineligible responses, and no leakage of emails, private answers, exact scores, sensitive failure reasons, storage keys, or another viewer's items. Test authorized image access if included.
- Build the affected projects and solution; run relevant tests. Describe required model/schema changes for the user-managed migration; do not generate it.

## Open implementation decisions
- Choose the exact minimal card fields and localization before defining the response. Do not infer a public display name from an email address.
- Choose how a removed unseen item is represented in storage, such as deletion or a non-unseen state, without recording a false pass. Clarify whether the same candidate can later be recommended again after becoming eligible. Any schema change needs a user-managed migration.
- Decide the scheduler retry interval, bounded work size, and endpoint rate limit before implementing those pieces. These are operational settings, not ranking signals.
