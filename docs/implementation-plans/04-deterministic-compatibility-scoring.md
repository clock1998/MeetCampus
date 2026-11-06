# Phase 4: Deterministic Compatibility Scoring

## Goal
Implement a server-only, deterministic matching-rules engine that calculates compatibility and evidence coverage from validated questionnaire answers and connection intents. Keep eligibility and safety constraints separate from numerical scoring, and generate explanations only from explicitly approved, shareable evidence. Do not use ML or popularity signals.

## Prerequisites and current state
- Phase 3 onboarding, questionnaire responses, and central eligibility checks are in place. Recommendation and contact enforcement remain future integrations; the recommendation availability endpoint does not generate or return matches.
- `MeetCampus/Data/Questionnaire/Question.cs` stores a stable question ID, category `Group`, question `ScoringWeight`, options, and `UpdatedAt`. `UserAnswer` stores current answers and their update timestamps; answer history is not retained.
- `MeetCampus/Features/Configuration/MatchingWeightsOptions.cs` defines the five category weights and validates that they sum to 1. `IntentCompatibilityRule` stores directional sender/recipient intent pairs.
- The repository does not yet provide a scoring engine, published question-specific scoring matrices, a complete intent matrix, question privacy classifications, or an approved explanation-template catalog. Confirm the initial questionnaire and these configuration inputs are published before using them to generate recommendations.
- `docs/mvp-spec.md` is the MVP source of truth. This phase does not implement Phase 5 batch generation or Phase 6 recommendation decisions.

## Features

### Feature 1: Versioned scoring definitions and approved configuration

**Outcome:** The engine has a validated, immutable set of definitions for category weights, question-level scoring rules, intent rules, privacy classification, and approved explanation templates.

**Implementation:**
1. Define a server-owned, versioned scoring-configuration snapshot containing algorithm and configuration versions, five category weights, stable question/option identifiers, question weights and types, and the rule parameters needed to compare answers. Keep persistence entities and snapshots out of shared contracts.
2. Map existing questionnaire category codes (`interests`, `values`, `social`, `lifestyle`, `connection-intent`) explicitly to the five `MatchingWeightsOptions` values. Reject unsupported categories, invalid weights, malformed matrices, and ambiguous or incomplete rules rather than silently substituting defaults.
3. Define an explicit privacy classification and scoring/display permissions for each configured question and approved explanation template. Default to non-shareable when approval or classification is missing; never infer privacy from a prompt or option label.
4. Define how approved configurations and the directional intent matrix are published and versioned, including validation that all referenced questions, options, intent IDs, scoring rules, and template inputs exist. Retain the exact version or reproducible immutable reference required to interpret earlier results after a definition changes.
5. Identify entity and schema changes required for versioned rules, privacy metadata, or templates. The user manages migrations: document the required schema impact and do not create or modify migration files unless explicitly requested.

**Acceptance criteria:**
- A configuration with unknown categories, invalid weights, missing references, or unsupported rule parameters cannot be published or used for scoring.
- A fixed configuration version resolves to the same rules even after current questionnaire definitions change.
- Unclassified or unapproved data cannot become explanation output.

### Feature 2: Server-side scoring input preparation

**Outcome:** A matching operation can construct a bounded, immutable, server-only input from persisted question definitions, validated current answers, selected intents, and the selected scoring configuration.

**Implementation:**
1. Create focused input records under the server matching feature for the viewer and candidate, configured questions, typed answer values, selected intent IDs, and versioned scoring settings. Keep EF entities, database access, HTTP, time, and localization out of the pure engine.
2. Load relevant data with read-only EF Core queries and cancellation propagation, using stable IDs rather than localized text. Map each `UserAnswer` group into the configured typed answer shape.
3. Reuse `QuestionnaireAnswerRules.IsValidStoredAnswer` and the existing freshness rule: an answer is usable only when its stored shape is valid and all relevant answer timestamps are at least the question's `UpdatedAt`. Treat absent, invalid, or stale answers as unavailable evidence, never as disagreement.
4. Define an explicit policy for a mismatch between current question definitions and the requested immutable scoring configuration; do not silently compare answers using a different version of the rules.
5. Order questions and option values by stable keys before handing them to the engine. Do not log raw answers or place them in client contracts.

**Acceptance criteria:**
- Equivalent persisted data produces equivalent scoring inputs regardless of database row order or locale.
- Missing, stale, and invalid answers are excluded from usable comparisons and cannot be mistaken for negative evidence.
- The pure scoring input contains no EF entities or client-supplied eligibility claims.

### Feature 3: Hard constraints and directional intent compatibility

**Outcome:** A pair must pass explicit, independently evaluated hard constraints before any score can qualify the pair for recommendation.

**Implementation:**
1. Implement a pure intent evaluator using selected intent IDs and the published directional sender-to-recipient matrix. Dating is compatible only when both users selected dating; evaluate other intent pairs in the viewer-to-candidate direction.
2. Define behavior for multiple selected intents and missing matrix entries according to approved product rules. Reject or fail closed for an unpublished or incomplete matrix; do not infer reciprocal non-dating rules.
3. Implement separately configured hard-constraint evaluators once their catalog and semantics are approved. Return internal pass/fail results without exposing a sensitive reason to another member.
4. Define the server orchestration boundary that checks both users' current eligibility via `AccountEligibilityService`, intent compatibility, hard constraints, blocks, and moderation restrictions before scoring. Recheck relevant eligibility and safety immediately before a future API returns a profile or processes interest; do not duplicate the central account policy.
5. Leave candidate queries, batch construction, contact operations, and their endpoint integration to their respective later phases. Document any missing block or hard-constraint implementation as a prerequisite rather than treating it as passed.

**Acceptance criteria:**
- One-sided dating selection cannot establish dating compatibility; directional non-dating rules are not assumed to be symmetric.
- Incompatible intent, a failed constraint, a block, or an applicable moderation restriction cannot be overridden by a high score.
- Missing required safety or intent configuration fails closed without disclosing private member state.

### Feature 4: Pure compatibility scoring and evidence coverage

**Outcome:** Given immutable inputs and an approved configuration, one pure engine calculates reproducible category scores, a final score, and independent evidence coverage.

**Implementation:**
1. Implement question-specific comparison rules using approved matrices or normalized distance/overlap functions appropriate to each question. Document whether each rule is symmetric or directional; do not apply a generic agreement formula to every question type.
2. Calculate each category score on a 0–100 scale from usable joint answers and approved question weights. Apply category weights of 0.25 interests, 0.30 values, 0.20 social/communication, 0.15 lifestyle/availability, and 0.10 connection intent/expectations by default, using validated configuration rather than hard-coded values.
3. Calculate the final score as `sum(categoryWeight[k] * categoryScore[k])`, according to the approved policy for categories without usable joint evidence. Specify decimal precision, rounding, result range, and behavior when no comparable evidence exists before implementation; do not assign missing answers a zero score or invent a neutral score.
4. Calculate coverage independently as `sum(categoryWeight[k] * jointlyAnsweredFraction[k]) / sum(categoryWeight[k])`, where each fraction is the proportion of relevant questions in that category with usable answers from both users. Define how empty categories are handled as part of the published configuration policy.
5. Return internal category results, final score, coverage, and version identifiers without exposing exact scores or raw answers through public APIs. Use deterministic ordering and culture-independent numeric calculations.

**Acceptance criteria:**
- Fixed inputs and configuration versions produce identical results across repeated runs and input ordering.
- Missing data decreases evidence coverage as applicable but is not scored as disagreement; score and coverage remain distinct.
- Category and final results follow approved boundaries, weights, precision, and rounding rules, including zero-evidence cases.

### Feature 5: Privacy-safe bands and grounded explanations

**Outcome:** Matching can produce a qualitative band, coverage indicator, and deterministic explanation candidates from explicitly approved evidence without exposing exact scores or protected answers.

**Implementation:**
1. Map internal scores to qualitative bands using published, validated thresholds and labels. Keep exact numeric scores server-side and localize any eventual user-facing labels.
2. Produce explanation candidates only when a configured template is approved, its referenced evidence is current and shareable, and its predicate is supported by both answers. Never use private or exact matching-only answer values, sensitive-trait inference, or arbitrary generated text.
3. Select candidates deterministically using stable template and evidence keys and the approved selection policy. Provide two or three grounded commonalities and an optional conversation starter only when enough approved evidence supports them.
4. Define and approve the safe behavior when fewer than two shareable commonalities exist; do not fabricate explanations or leak why evidence is unavailable. Keep rendering and recommendation contracts in the later recommendation feature.

**Acceptance criteria:**
- The same input and configuration yield the same band and explanation candidate ordering.
- Unapproved, private, matching-only, stale, or unsupported evidence never appears in an explanation or conversation starter.
- No client-facing output exposes exact scores or claims that the score predicts relationship success.

### Feature 6: Reproducibility and downstream integration contract

**Outcome:** Later recommendation features can invoke scoring safely and retain sufficient versioned metadata to reproduce or audit a result without preserving response history.

**Implementation:**
1. Define an internal result carrying algorithm and scoring-configuration versions, category scores, final score, coverage, and approved explanation references. Keep internal diagnostics separate from privacy-safe client output.
2. Specify what a future recommendation batch must persist: algorithm and scoring configuration versions, question/rule version or reproducible immutable reference, score, coverage, category-score inputs or a reproducible reference, and explanation-template inputs. Phase 5 owns batch IDs, timestamps, selection seed, ranking, and decision state.
3. Because current questionnaire answers are overwritten, explicitly resolve how to reproduce historical results without retaining answer history: for example, persist an approved minimal immutable scoring-input snapshot or another demonstrably reproducible reference. Define access control, retention, privacy implications, and schema impact before choosing the representation.
4. Document how Phase 5 will compose current eligibility/safety checks, this engine, the configured minimum compatibility score, and deterministic ranking; Phase 6 must recheck both users' eligibility and intent before interest/contact transitions.

**Acceptance criteria:**
- Historical result metadata identifies the exact rules used and can reproduce the result under the approved retention model, even after current answers or definitions change.
- The integration boundary does not expose persistence entities, exact scores, or private input data to the client.
- No recommendation/contact endpoint is claimed to be fully enforced until its actual entry point performs the required checks.

## Decisions required before implementation
- Publish question-specific comparison matrices/functions, question weights, privacy classes, supported hard constraints, intent-matrix entries, and approved localized templates.
- Define multi-intent matching semantics, the handling of missing intent rules, and how dating is identified using stable configuration rather than localized labels.
- Approve category normalization, empty-category/no-evidence behavior, precision and rounding, band labels and thresholds, and explanation fallback behavior.
- Choose the versioning and reproducibility approach compatible with current-answer-only storage and privacy/retention requirements; identify any required user-managed schema migration.
- Confirm whether definition freshness is evaluated against current definitions or a specific published snapshot when the two differ.

## Non-negotiable rules
- Missing answers contribute no disagreement, and insufficient evidence must not be presented as incompatibility.
- Never use likes received, profile views, message volume, response rate, time spent, or other engagement/popularity signals.
- Never expose exact scores, private answers, matching-only answers, sensitive-trait inferences, or unsupported claims.
- Hard constraints, eligibility, intent, blocks, and safety restrictions take precedence over numerical scores.

## Cross-feature validation and phase exit criteria
- Unit tests cover every approved comparison rule, all five categories, configured weights, normalization, rounding and boundaries, evidence coverage, empty categories, and missing/stale/invalid answers.
- Unit tests cover symmetric dating, directional non-dating rules, multi-intent behavior, missing matrix entries, and hard-constraint precedence.
- Explanation tests cover template approval, privacy classifications, grounded evidence, deterministic ordering, and insufficient-shareable-evidence behavior.
- Integration tests cover input mapping, current-answer freshness, versioned configuration loading, and the approved reproducibility mechanism without exposing private data.
- A fixed input and version yield exactly the same score, coverage, band, and explanation candidates. Build affected projects and the solution and run focused tests; migrations remain user-managed.
