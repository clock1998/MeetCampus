# Phase 4: Deterministic Compatibility Scoring

## Goal
Build a simple server-side service that compares two members' answers and calculates a compatibility score. Show how much information supports the score, and explain a match using only information approved for sharing. The same answers and rules must always give the same result.

## Prerequisites and current state
- Phase 3 provides profile setup, questionnaire answers, and account eligibility checks. The recommendation availability endpoint does not create or return recommendations.
- Questions already have categories, weights, options, and update times. Only current answers are stored; old answers are not kept.
- `MatchingWeightsOptions` contains the five category weights. `IntentCompatibilityRule` stores which connection intents may match in each direction.
- Scoring rules, complete intent rules, question privacy settings, and approved explanation text still need to be defined.
- Follow `docs/mvp-spec.md`. Recommendation batches and decisions are part of later phases.

## Features

### Feature 1: Set up scoring rules

**Outcome:** The service knows how to compare each question and which information it may share.

**Implementation:**
1. Define a simple comparison rule for each question type and any question that needs special handling. Use stable question and option IDs, not translated text.
2. Connect the existing question categories (`interests`, `values`, `social`, `lifestyle`, `connection-intent`) to `MatchingWeightsOptions`. Reject unknown categories, invalid weights, and incomplete rules.
3. Mark which answers may be used for scoring and which may appear in explanations. Treat answers as private unless sharing is explicitly approved. Add approved explanation text only where needed.
4. Give each set of scoring rules a version. Decide how to keep the rules needed to understand older results when questions or settings change.
5. Describe any required database changes. Migrations are managed by the user; do not create or modify them unless requested.

**Acceptance criteria:**
- Invalid or incomplete rules cannot be used for scoring.
- Older results can be associated with the rules used to calculate them.
- Private or unapproved information is never used in explanations.

### Feature 2: Load answers for scoring

**Outcome:** The server collects the current answers and selected intents needed to compare two members.

**Implementation:**
1. Add simple server-only data types for the two members' answers, intents, and scoring rules. Keep database access separate from the calculation.
2. Load questions and answers using read-only database queries, and pass cancellation tokens through database calls.
3. Reuse `QuestionnaireAnswerRules.IsValidStoredAnswer`. Use an answer only if it is valid and was updated at or after the question's `UpdatedAt`. Ignore missing, invalid, or outdated answers; do not count them as disagreements.
4. Decide what to do if the current question no longer matches the scoring rules. Never silently score an answer using the wrong rules.
5. Use stable IDs and a consistent order. Do not log answers or send them to the client as part of scoring.

**Acceptance criteria:**
- The same saved answers give the same inputs regardless of database row order or language.
- Missing, outdated, and invalid answers are not counted as disagreements.
- The calculation does not depend on database entities or client-provided eligibility flags.

### Feature 3: Check intent and safety rules

**Outcome:** Members who should not be matched are excluded even if their answers would give a high score.

**Implementation:**
1. Compare selected intents using the configured rules. Both members must select dating for a dating match; check other intents from the viewer's choice toward the candidate's choice.
2. Agree on how to handle multiple selected intents. If required intent rules are missing, do not allow the match. Do not assume a rule works in both directions.
3. Add any required match restrictions only after their rules have been agreed. Do not reveal private reasons for excluding a member.
4. Before a later feature creates or returns a recommendation, check both members' eligibility with `AccountEligibilityService`, intent rules, blocks, and moderation restrictions. Check again before returning a profile or processing interest.
5. Build recommendation lists and contact operations in their later phases. Do not treat missing safety checks as passed.

**Acceptance criteria:**
- Dating requires both members to select dating; other intent rules can differ by direction.
- A high score cannot override an incompatible intent, block, moderation restriction, or other required restriction.
- Missing rules do not allow a match or reveal private information.

### Feature 4: Calculate the score and answer coverage

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

**Outcome:** Members see a simple match label and explanations based only on approved information.

**Implementation:**
1. Convert the score to an agreed label without showing the exact number. Show an indication of how many questions both members answered; translate labels when they are displayed.
2. Use approved text only when both members' current answers support it and the information is approved for sharing. Never expose private answers, exact matching-only answers, or guessed sensitive traits.
3. Choose explanations in a consistent order. Aim for two or three shared points and an optional conversation starter when the available information supports them.
4. Agree on what to show when fewer than two shared points are available. Never invent an explanation. The recommendation page is part of a later phase.

**Acceptance criteria:**
- The same answers and rules produce the same label and explanations.
- Explanations never disclose private or matching-only answers or make unsupported claims.
- Members never see exact scores or claims that a score predicts relationship success.

### Feature 6: Prepare for recommendations

**Outcome:** Later phases can use the score safely and identify the rules used for each recommendation.

**Implementation:**
1. Return the score, category results, coverage, rule versions, and approved explanations to server code. Keep private details out of client responses.
2. Record what Phase 5 needs to save for each recommendation: the rule versions, score, coverage, information needed to explain and reproduce the score, and explanation inputs. Phase 5 also owns batch dates, ordering, selection seed, and decisions.
3. Decide how to reproduce an older score when members can change their answers, without introducing answer history by accident. Agree on what to save, who can access it, how long to keep it, and any database changes before implementation.
4. Phase 5 must check eligibility and safety before using scores to select recommendations. Phase 6 must check eligibility and intent again before processing interest.

**Acceptance criteria:**
- Saved recommendations identify the rules used and include enough approved information to reproduce their scores even after answers change.
- Client responses do not expose database entities, exact scores, or private answers.
- Recommendation and contact entry points are not considered protected until their own checks are implemented.

## Decisions required before implementation
- Agree on question comparison rules, question weights, which answers can be shared, intent rules, match restrictions, and explanation text.
- Decide how multiple intents are matched, how missing rules are handled, and how dating is identified without relying on translated labels.
- Decide how to score when answers or categories are missing, how to round scores, which labels to use, and what to show when there are too few explanations.
- Decide how to save enough information to reproduce older scores without keeping answer history, and identify any database changes needed.
- Decide which question version determines whether an answer is up to date.

## Non-negotiable rules
- Missing answers are not disagreements, and limited information does not mean members are incompatible.
- Never use likes, profile views, message counts, response rates, time spent, or similar activity to calculate scores.
- Never expose exact scores, private answers, matching-only answers, guessed sensitive traits, or unsupported claims.
- Eligibility, intent, blocks, and safety rules always take priority over the score.

## Cross-feature validation and phase exit criteria
- Test all comparison rules, category weights, score limits, rounding, coverage, and missing or outdated answers.
- Test dating, other intent rules in both directions, multiple intents, missing rules, and safety restrictions.
- Test that explanations use only approved information and remain consistent for the same answers.
- Test loading current answers and the agreed way of checking older scores without exposing private data.
- Build the affected projects and solution, and run the relevant tests. The user manages database migrations.
