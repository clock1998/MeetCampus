# Phase 4: Deterministic Compatibility Scoring

## Goal
Implement a pure, matching-rules engine that calculates compatibility and evidence coverage without ML or popularity signals.

## Prerequisites
- Phase 3 complete.
- Initial questionnaire, question privacy classes, scoring configuration, explanation templates, and intent matrix are published.

## Increments
1. Define server-only scoring input models built from versioned structured answers and intent data; exclude private/non-shareable values from explanation inputs.
2. Implement one pure scoring service that calculates the five weighted category scores and final 0–100 score.
3. Implement evidence coverage as the weighted proportion of relevant questions answered by both users, independently of compatibility.
4. Add hard-constraint and intent-compatibility evaluators outside the numerical scoring calculation. Dating requires both users to select it; other intents use the directional matrix.
5. Implement qualitative score-band mapping and deterministic explanation selection using only approved, grounded commonalities.
6. Persist algorithm/version identifiers or reproducible scoring references for every future recommendation evaluation.

## Non-negotiable rules
- Missing answers contribute no disagreement.
- Never use likes received, profile views, message volume, response rate, time spent, or any engagement/popularity signal.
- Never infer sensitive traits.
- Do not expose exact score, private answers, matching-only answers, or unsupported claims.

## Tests and exit criteria
- Unit tests cover each category, configured weights, rounding/boundaries, evidence coverage, and missing-answer behavior.
- Unit tests cover symmetric dating and directional non-dating intent rules.
- Explanation tests prove that only approved shareable inputs are emitted.
- A fixed input and version yield exactly the same score, coverage, band, and explanation candidates.
