# Phase 4: Deterministic Compatibility Scoring

## Goal
Build a simple server-side service that compares two members' answers and calculates a compatibility score. Show how much information supports the score, and explain a match using only information approved for sharing. The same answers and rules must always give the same result.

## Prerequisites and current state
- Phase 3 provides profile setup, questionnaire answers, and account eligibility checks. The recommendation availability endpoint does not create or return recommendations.
- Questions already have categories, weights, options, and update times. Only current answers are stored; old answers are not kept.
- `MatchingWeightsOptions` contains the five category weights. The matching check uses intent rules in code; it does not read the existing `IntentCompatibilityRule` table.
- Answer comparison, answer loading, and pair checks are implemented. Overall scores, answer coverage, match labels, and explanations are not yet implemented.
- Recommendation batches and decisions are part of later phases. Scoring rules are kept in code without versions or evaluation history, as agreed for this MVP; this differs from the version and historical reproduction requirements in `docs/mvp-spec.md`.

## Features

### Feature 1: Set up scoring rules

**Status:** Answer comparison rules and category-weight lookup are implemented. Match explanations are pending in Feature 5.

**Outcome:** The server can compare answers for the supported question types.

**Implementation:**
1. `AnswerScoringRuleService` compares single choices by option ID: identical choices score 100; different choices score 0.
2. Multiple choices use the Sørensen–Dice coefficient: `200 × shared options / (first option count + second option count)`. Scale answers use `100 × (1 − distance / scale range)`.
3. `MatchingWeightsOptions` maps `interests`, `values`, `social`, `lifestyle`, and `connection-intent` to their configured weights. It rejects unknown categories and invalid weights.
4. Comparison rules are in server-side code. They do not store evaluations or require scoring rules to be stored in the database. Existing configuration and recommendation models still contain version fields; the comparison service does not use them.
5. Generic category-level explanations are planned in Feature 5. They must not reveal individual answers. No database changes were needed for the comparison rules.

**Acceptance criteria:**
- The comparison methods reject invalid inputs, including empty or repeated choices and invalid scale ranges.
- Scoring rules are implemented in code; comparisons do not store or version evaluations.
- Explanations must not reveal private or unapproved information; this remains to be implemented.

### Feature 2: Load answers for scoring

**Status:** The server-side loader is implemented. Stable ordering for questions with the same sort order still needs attention.

**Outcome:** The server collects the current answers and selected intents needed to compare two members.

**Implementation:**
1. `ScoringInputService` loads question details, both members' answers, and their selected intent IDs using read-only queries and cancellation tokens. Its result uses server-only data types, not database entities.
2. It reuses `QuestionnaireAnswerRules.IsValidStoredAnswer` and requires every answer row to be at least as recent as the question's `UpdatedAt`. Missing, invalid, duplicate-choice, or outdated answers are returned as unavailable.
3. Supported questions remain in the result even when an answer is unavailable, so answer coverage can be calculated later. Unsupported question types and invalid scale ranges are omitted.
4. Option and intent IDs are sorted. Questions are currently ordered by `SortOrder` only; add a stable ID tie-breaker before relying on consistent ordering when questions share the same sort order.
5. Keep answer data on the server and do not log it or expose it through scoring responses.

**Acceptance criteria:**
- The same saved answers must give the same inputs regardless of database row order or language; question ordering still needs a tie-breaker.
- Missing, outdated, and invalid answers are not counted as disagreements.
- The calculation does not depend on database entities or client-provided eligibility flags.

### Feature 3: Check intent and safety rules

**Status:** The pair check and the two-intent selection limit are implemented. Future recommendation and contact operations must call the pair check.

**Outcome:** Members who should not be matched are excluded even if their answers would give a high score.

**Implementation:**
1. Profile setup requires one or two distinct, valid intents on the server. The client shows the limit in English and French and prevents selecting a third intent.
2. `IntentCompatibilityService` checks intent codes using rules in code: identical supported intents match; different intents do not. This means dating matches only when both members select dating. Unknown intent codes cause a configuration error.
3. With multiple selected intents, each direction needs at least one allowed pair, and both directions must pass. The current rules are the same in both directions.
4. `MatchEligibilityService` rejects self-matches, checks each member through `AccountEligibilityService`, checks blocks in both directions, and then checks intents. The account check includes applicable moderation restrictions and recipient capacity. The pair service returns only a yes/no result.
5. Recommendation and contact operations do not yet use this pair check. Later features must call it and recheck eligibility and safety before returning a profile or processing interest. Additional match restrictions remain pending until their rules are agreed.

**Acceptance criteria:**
- Dating requires both members to select dating. Current intent rules are identical in both directions.
- A high score cannot override an incompatible intent, block, moderation restriction, or other required restriction.
- Missing rules do not allow a match or reveal private information.

### Feature 4: Calculate the score and answer coverage

**Status:** Pending. Individual answer comparisons exist, but category scores, overall score, and coverage do not.

**Outcome:** One calculation produces a score and, separately, how many relevant questions both members answered.

**Implementation:**
1. Compare valid answers using the approved rule for each question. A shared interest can count positively without treating every different interest as a disagreement.
2. Calculate a score from 0 to 100 for each category using its question weights. Use the existing default category weights: interests 25%, values 30%, social preferences 20%, lifestyle 15%, and connection intent 10%.
3. Calculate the overall score by multiplying each category score by its weight and adding the results. Agree on rounding and how to handle categories with no comparable answers; do not treat missing answers as a zero score.
4. Separately calculate answer coverage: for each category, find the fraction of relevant questions answered by both members, then combine those fractions using the category weights. Agree on how to handle categories with no relevant questions.
5. Keep exact scores and answers on the server. Return the same result whenever the answers and rules are unchanged.

**Acceptance criteria:**
- The same answers and rules give the same result, regardless of input order.
- Missing answers reduce coverage where applicable but do not count as disagreement.
- Scores, coverage, and rounding follow the agreed rules, including when no answers can be compared.

### Feature 5: Explain a match

**Status:** Pending. Use generic category-level explanations rather than exposing individual answers.

**Outcome:** Members see a simple match label and explanations based only on approved information.

**Implementation:**
1. Convert the score to an agreed label without showing the exact number. Show an indication of how many questions both members answered; translate labels when they are displayed.
2. Use generic category-level text, such as a similarity in values, only when both members' current answers support it and the information is approved for sharing. Never expose private answers, exact matching-only answers, or guessed sensitive traits.
3. Choose explanations in a consistent order. Aim for two or three shared points and an optional conversation starter when the available information supports them.
4. Agree on what to show when fewer than two shared points are available. Never invent an explanation. The recommendation page is part of a later phase.

**Acceptance criteria:**
- The same answers and rules produce the same label and explanations.
- Explanations never disclose private or matching-only answers or make unsupported claims.
- Members never see exact scores or claims that a score predicts relationship success.

### Feature 6: Prepare for recommendations

**Status:** Pending. The existing recommendation item model has score and coverage fields, but recommendations are not generated or saved yet.

**Outcome:** Later phases can use the score safely without versioning or storing every evaluation.

**Implementation:**
1. Return the score, category results, coverage, and approved explanations to server code. Do not save each evaluation or keep an evaluation history. Keep private details out of client responses.
2. Phase 5 may save the score, coverage, and approved explanation text needed to show a selected recommendation. Do not store scoring rules or their versions with the recommendation.
3. Do not build a feature to reproduce old scores after answers or scoring code change. Keep only the recommendation data needed by the MVP.
4. Phase 5 must check eligibility and safety before using scores to select recommendations. Phase 6 must check eligibility and intent again before processing interest.

**Acceptance criteria:**
- Selected recommendations save only the result information needed to show them to members.
- Individual evaluations are not versioned or stored as a history.
- Scoring rules remain in code and have no database version.
- Client responses do not expose database entities, exact scores, or private answers.
- Recommendation and contact entry points are not considered protected until their own checks are implemented.

## Decisions required before implementation
- Agree on any additional match restrictions, which information may support generic explanations, and the explanation text.
- Decide how to score when answers or categories are missing, how to round scores, which labels to use, and what to show when there are too few explanations.
- Decide which score, coverage, and explanation data Phase 5 must save to show selected recommendations.
- Decide how to handle question ordering ties before relying on a consistent order for scoring.

## Non-negotiable rules
- Missing answers are not disagreements, and limited information does not mean members are incompatible.
- Never use likes, profile views, message counts, response rates, time spent, or similar activity to calculate scores.
- Never expose exact scores, private answers, matching-only answers, guessed sensitive traits, or unsupported claims.
- Eligibility, intent, blocks, and safety rules always take priority over the score.

## Cross-feature validation and phase exit criteria
- Test all comparison rules, category weights, score limits, rounding, coverage, and missing or outdated answers.
- Test dating, other intent rules in both directions, multiple intents, missing rules, and safety restrictions.
- Test that explanations use only approved information and remain consistent for the same answers.
- Test that current answers are loaded and that no evaluation history or scoring-rule database storage is introduced.
- Build the affected projects and solution, and run the relevant tests. The user manages database migrations.
