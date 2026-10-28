# University Compatibility Matching System MVP

## Product Requirements and Implementation Scope

**Document version:** 1.0  
**Status:** MVP implementation specification  
**Audience:** Product, design, engineering, QA, and AI coding assistants  
**Product type:** Free, non-profit university community platform  
**Scope:** One university pilot

---

## 1. MVP Goal

Validate a finite, explainable, consent-based matching loop for university students:

1. A student verifies a university email address.
2. The student completes a required profile, uploads a profile image, and answers the baseline questionnaire.
3. The system delivers a limited daily batch of compatible people.
4. Each person can express interest or pass.
5. A mutual interest opens a conversation.
6. Every profile and conversation supports blocking and reporting.

The MVP must not use a swipe interface, infinite feed, popularity ranking, engagement ranking, or machine learning.

### 1.1 MVP success hypothesis

Students will find finite, grounded recommendations more useful and safer than unrestricted profile browsing when recommendations explain meaningful commonalities and contact requires mutual interest.

### 1.2 MVP non-goals

Defer the following until the core loop is validated:

- Custom discovery, search filters, pagination, and exposure balancing.
- Activity recommendations, meeting planning, events, and group activities.
- Evidence attachments for reports.
- Appeals, complex moderation workflows, and automated moderation.
- Advanced analytics and fairness dashboards.
- Machine learning, behavioral ranking, popularity signals, or psychological inference.
- A general-purpose administrator UI for every setting; use validated application configuration and restricted administrative data management where sufficient.

---

## 2. MVP Product Invariants

1. **Compatibility over popularity:** Do not use likes received, profile views, message volume, response rate, attractiveness ratings, or time spent as ranking signals.
2. **Finite discovery:** Deliver a small daily recommendation batch. Do not implement an infinite feed.
3. **Explainability:** Show grounded, shareable reasons for every recommendation.
4. **Mutual consent:** Unrestricted messaging begins only after both eligible users express interest.
5. **Safety:** Blocking and reporting must be available before public pilot release.
6. **Privacy:** University email addresses and private questionnaire answers are never returned in public-profile, recommendation, or conversation APIs.
7. **Deterministic matching:** Use a versioned rules-and-weight engine. Do not introduce ML in the MVP.
8. **Server enforcement:** Eligibility, privacy, blocks, quotas, and messaging authorization are enforced on the server and, where appropriate, in the database.

---

## 3. Roles

- **Student:** Registers, verifies email, completes profile/questionnaire, receives recommendations, expresses interest or passes, messages mutual connections, blocks, and reports.
- **Moderator:** Reviews reports and applies an MVP action: no action, warning, temporary messaging restriction, temporary suspension, or permanent removal.
- **Administrator:** Maintains allowed university email domains, supported intent rules, questionnaire versions, and configured limits. Administrative actions must be auditable.
- **System worker:** Generates recommendation batches, expires pending requests, and runs required retention or cleanup jobs.

---

## 4. Account Eligibility and Onboarding

### 4.1 University verification

- The pilot supports one university.
- An administrator-managed allowlist of university email domains is stored in the application database.
- The server validates allowed domains and verification tokens.
- Normalize email addresses and enforce uniqueness.
- Do not expose university email addresses through public APIs.
- Rate-limit verification and recovery attempts and avoid account enumeration.
- Use dummy/no-op email delivery for the MVP; production email delivery is deferred.

### 4.2 Discoverability prerequisites

A student may become discoverable only when all of the following are true:

- Email is verified.
- Account is active and not suspended, deactivated, or paused.
- The student explicitly opts into discoverability.
- All required profile fields are complete.
- At least one profile image has been uploaded.
- All active baseline questionnaire questions are answered.

The system must immediately remove a user from recommendations and contact eligibility when any prerequisite stops being true.

### 4.3 Profile fields

Every MVP field below is required unless explicitly marked optional. Age or age range is deferred; do not make it an MVP profile field or discoverability prerequisite.

- Display name.
- Connection intents.
- Interests and activities.
- Social and communication preferences represented through approved user summaries.
- At least one profile image.
- Program/faculty (optional).
- Short biography (optional).
- Ideal activities or first-meetup preferences (optional).
- Conversation starter (optional).
- Coarse availability, never a live schedule (optional).

---

## 5. Questionnaire and Matching

### 5.1 Questionnaire

- Start with approximately 15–25 baseline questions across interests, values, social preferences, lifestyle, availability, and connection expectations.
- An administrator configures question definitions, validation, privacy class, scoring configuration, and approved explanation templates.
- Each question has an `UpdatedAt` timestamp. When a question changes, answers submitted before that timestamp can be marked as needing an update; prior answers are not silently rewritten.
- Store stable question IDs and validated structured answers.
- Missing answers are not disagreements.
- Do not infer sensitive traits from profiles, photos, writing, hobbies, or answer patterns.
- Sensitive traits are not categorically excluded from required profile or questionnaire fields; any included field must be explicitly defined in the approved configuration, with its privacy class and scoring/display rules specified.
- Optional deeper questions may be offered after onboarding but are not required for discoverability.

### 5.2 Connection intents

The pilot enables friendship, dating, study partners, and activity partners.

- Users may choose multiple intents.
- Dating is symmetric: both users must select dating for dating eligibility.
- Other intent compatibility is directional and evaluated using the administrator-configured intent compatibility matrix.
- Users may restrict which intents can send them interests or introduction requests.

### 5.3 Compatibility score

Use a deterministic weighted score from 0 to 100:

```text
compatibility_score(u, v) =
	sum(weight[k] * category_score[k](u, v))
```

Initial configurable weights:

| Category | Weight |
| --- | ---: |
| Interests and activities | 0.25 |
| Values and priorities | 0.30 |
| Social and communication preferences | 0.20 |
| Lifestyle and availability | 0.15 |
| Connection intent and expectations | 0.10 |
| **Total** | **1.00** |

Hard constraints, blocks, moderation restrictions, and intent incompatibility are evaluated separately and always override a high score.

Calculate evidence coverage separately from compatibility. It represents the weighted proportion of relevant questions answered by both people. Do not treat lower evidence coverage as disagreement.

### 5.4 Recommendation explanations

Every recommendation includes:

- A qualitative compatibility band, never an exact score.
- An evidence coverage indicator.
- Two or three deterministic, grounded commonalities based only on approved shareable information.
- Optional constructive conversation starter where supported by shareable data.

Do not expose private answers, exact matching-only answers, inferred sensitive traits, or unsupported claims.

---

## 6. Daily Recommendations

### 6.1 Batch behavior

Initial configurable defaults:

- 3 new recommendations per user per day.
- 9 maximum unseen recommendations.
- Daily refresh cadence.
- Ninety-day suppression after a pass.
- Thirty-point minimum compatibility score for inclusion.

A candidate must pass all eligibility, intent, hard-constraint, block, and moderation checks before inclusion.

A person omitted from a batch is not necessarily incompatible.

### 6.2 Reproducibility

Record the following for every batch and recommendation:

- Batch identifier and timestamp.
- Viewer and candidate identifiers.
- Algorithm and scoring configuration versions.
- Compatibility score and evidence coverage.
- Category-score inputs or a reproducible versioned reference.
- Explanation-template inputs.
- Selection seed.
- Recommendation decision: `unseen`, `interested`, `passed`, or `saved`.

The MVP may rank primarily by compatibility and use a deterministic seeded tie-breaker. Do not implement advanced exposure balancing until custom discovery is introduced.

---

## 7. Mutual Interest and Messaging

### 7.1 Interest behavior

`Interested` is a private mutual-interest signal.

- An eligible user may express interest in an eligible recommendation.
- Interest does not immediately permit unrestricted messaging.
- When both users have active, intent-compatible interest signals, the system creates a connection and opens a conversation.
- Passing suppresses the candidate for the configured period.
- Blocks immediately terminate discovery, mutual-interest eligibility, and messaging in both relevant directions.

### 7.2 Introduction/connection states

Use explicit states for the mutual-contact lifecycle:

- `pending`: one-sided interest exists; no unrestricted messaging.
- `accepted`: mutual interest exists; conversation is open.
- `declined`: recipient rejected a direct introduction where such a transition is used.
- `expired`: pending state exceeded the configured lifetime.
- `cancelled`: sender withdrew a pending interest or request.
- `blocked`: a block terminated contact eligibility.

For the MVP mutual-interest flow, transition `pending` to `accepted` only when the counterpart expresses compatible interest. Do not disclose whether another user passed, ignored, blocked, or was unavailable.

### 7.3 Capacity and quotas

Initial configurable defaults:

- Maximum three pending outbound contact attempts per user.
- Maximum five new inbound contact attempts per user per rolling seven days.
- Seven-day pending expiry.

Enforce quotas transactionally. When a recipient reaches their inbound capacity, temporarily remove them from discoverability. Restore discoverability automatically when the rolling count falls below capacity, provided all other eligibility requirements remain satisfied.

### 7.4 Messaging MVP

- Support basic text conversations only between accepted mutual connections.
- Recheck connection state, blocks, and account status on each send.
- Either participant may end the connection, leave the conversation, block, or report.
- Do not implement attachments, read receipts, typing indicators, message reactions, or message-content analytics in the MVP.

---

## 8. Safety and Moderation

### 8.1 Student controls

Every recommendation profile and conversation must provide:

- Block user.
- Report user.
- End connection/leave conversation.
- Mute notifications where notifications exist.

Report reasons:

- Harassment.
- Unwanted sexual content.
- Threats.
- Impersonation.
- Spam.
- Discrimination.
- Other misconduct.

Allow an optional text description. Defer evidence attachments.

### 8.2 Moderator workflow

1. Record the report, reason, reporter, reported account, timestamp, and optional description.
2. Apply immediate contact restrictions when required.
3. Allow authorized moderators to review the report and relevant enforcement history.
4. Record one action: no action, warning, messaging restriction, temporary suspension, or permanent removal.
5. Audit moderator actions.
6. Notify the reporting user without exposing private enforcement details.

### 8.3 Safety invariants

- Never reveal the reporter’s identity to the reported user.
- Blocks take effect immediately in recommendations, interest states, conversations, and profile access.
- Reports do not reduce the reporter’s compatibility score or visibility.
- Restrict moderator access to report data.
- Clearly state that the platform is not an emergency service.

---

## 9. MVP Data and API Requirements

- Use contracts from `MeetCampus.Contracts`; never return EF entities from APIs.
- Store email authentication data separately from public profile data.
- Enforce unique normalized email addresses.
- Use foreign keys for account, profile, questionnaire, interest, connection, message, block, and report relationships.
- Enforce unique directional blocks.
- Enforce unique active contact state per sender-recipient pair.
- Enforce quotas and state transitions transactionally.
- Index account status, discoverability, university eligibility, recommendation batch membership, pending contact states, and conversation message ordering.
- Store versioned structured questionnaire answers and versioned scoring metadata.
- Use Garage S3-compatible storage through the existing server-side storage service for profile images; never expose storage credentials to the client.

---

## 10. MVP Acceptance Criteria

Before the pilot, verify:

- [ ] Unverified, inactive, paused, suspended, or opted-out users cannot become discoverable.
- [ ] Discoverability requires all required profile fields, at least one uploaded profile image, and all baseline questionnaire answers.
- [ ] Blocked or otherwise ineligible candidates never appear in recommendations or contact flows.
- [ ] Recommendations use a compatibility score of at least 30 and show a qualitative band plus evidence coverage.
- [ ] Explanations use actual shareable input data and never leak private answers.
- [ ] Missing answers are not treated as disagreement.
- [ ] Popularity, profile views, message volume, response rate, and engagement time do not influence ranking.
- [ ] System batches respect the configured limits and are reproducible from recorded versions and seed.
- [ ] Daily batches provide at most three new recommendations per user per day and do not exceed nine unseen recommendations.
- [ ] Dating eligibility requires both users to select dating.
- [ ] Mutual interest is required before unrestricted messaging.
- [ ] Pending, accepted, expired, cancelled, and blocked contact transitions are validated server-side.
- [ ] Quotas remain correct under concurrent attempts.
- [ ] Recipient capacity removes a profile from discoverability temporarily and restores it when capacity is available.
- [ ] Blocks immediately prevent further contact and profile exposure.
- [ ] Reports are available from recommendations and conversations.
- [ ] Moderators can review reports and record audited actions.
- [ ] Public APIs never expose university email addresses, private answers, or storage credentials.
- [ ] Automated tests cover authorization, privacy, matching rules, state transitions, blocks, and quota concurrency.

---

## 11. Suggested Implementation Order

1. Review the existing hosted Blazor WebAssembly architecture and contracts.
2. Add account lifecycle, allowed-domain, discoverability, profile-image, questionnaire, and intent persistence models with migrations.
3. Implement verified-email and discoverability eligibility checks.
4. Implement profile and baseline questionnaire APIs and UI.
5. Implement pure deterministic compatibility scoring with unit tests.
6. Implement candidate eligibility, blocks, and recommendation batch generation.
7. Implement recommendation UI, explanation rendering, interest, pass, and batch decisions.
8. Implement mutual-interest connection state transitions, quotas, and messaging authorization.
9. Implement basic text conversation UI and API.
10. Implement block/report APIs, a restricted moderation queue, and audit events.
11. Add integration tests for privacy, authorization, state transitions, and concurrent quota enforcement.
12. Run a limited university pilot before expanding scope.

### Deferred future work (not part of MVP acceptance)

- Define an age or age-range field, the pilot age policy, and its validation/enforcement before adding age data collection.
- Define and implement a profile-image review/approval workflow before requiring approved-image status for discoverability.
- Replace dummy/no-op email delivery with an operational production email service.

---

## 12. Configuration

All values must be validated configuration, not scattered constants.

```yaml
matching:
  new_recommendations_per_user_per_day: 3
  max_unseen_recommendations: 9
  refresh_period: daily
  passed_profile_suppression_days: 90
  minimum_compatibility_score: 30
  use_popularity_signals: false
  display_exact_score: false
  weights:
    interests: 0.25
    values: 0.30
    social_communication: 0.20
    lifestyle_availability: 0.15
    connection_intent_expectations: 0.10

contact:
  max_pending_outbound: 3
  max_inbound_per_rolling_7_days: 5
  pending_expiry_days: 7
  require_mutual_interest_for_messaging: true
  remove_recipient_from_discoverability_at_capacity: true

privacy:
  university_email_public: false
  private_answers_in_public_profile: false
  private_answers_in_explanations: false
  discoverability_requires_user_opt_in: true
  profile_image_required_for_discoverability: true
  profile_image_approval_required_for_discoverability: false

safety:
  blocking_is_immediate: true
  reporting_required_for_mvp: true
  moderation_actions_audited: true
