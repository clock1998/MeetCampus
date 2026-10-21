# University Compatibility Matching System

## Product Requirements and Technical Specification

**Document version:** 1.0\
**Status:** Initial implementation specification\
**Audience:** Product, design, engineering, QA, and AI coding assistants
(GitHub Copilot)\
**Product type:** Free, non-profit university community platform\
**Initial scope:** One university; accounts verified through university
email

------------------------------------------------------------------------

## 1. Product Summary

Build a compatibility-first platform that helps university students
discover people with whom they may form meaningful connections. The
product must not use a swipe-based interface or optimize for endless
engagement.

The system should recommend people based on shared interests, values,
preferences, lifestyle compatibility, and compatible connection intent.
It should explain why each recommendation may be meaningful, provide
finite choices, respect consent, reduce unwanted messages, encourage
safe real-world activities, and provide robust misconduct reporting.

### 1.1 Supported connection types

The product should be configurable to support one or more of: -
Friendship - Dating / romantic connections - Study partners - Activity
partners - Other university community connections

Users may select multiple connection intents. Matching and contact
eligibility must respect intent compatibility.

### 1.2 Product principles

1.  **Compatibility over popularity:** Do not use likes received,
    profile views, message volume, response rate, or attractiveness
    ratings as compatibility or ranking features.
2.  **Finite discovery:** No infinite feed. System recommendations are
    delivered in limited batches. Custom discovery uses pagination and
    contact limits.
3.  **Explainability:** Every recommendation has grounded reasons based
    on shareable user-provided information.
4.  **Consent-based communication:** Unrestricted conversation starts
    only after the recipient accepts an introduction or both users
    otherwise express mutual interest.
5.  **Safety by default:** Blocking, reporting, moderation, account
    suspension, and privacy controls are MVP requirements.
6.  **Privacy by design:** University email and private questionnaire
    answers are never exposed through public profiles or APIs.
7.  **Fairness:** Monitor exposure and outcomes for systematic
    disparities. Avoid popularity feedback loops and hidden demographic
    proxies.
8.  **Real-world connection:** Offer optional shared-activity
    suggestions and meeting-planning tools without pressuring users to
    meet.
9.  **No engagement manipulation:** Do not optimize for time spent in
    the app, compulsive use, or message volume.
10. **Explainable, deterministic MVP:** Begin with a transparent
    rules-and-weight scoring model; do not introduce machine learning
    until there is evidence it is needed and can be governed safely.

------------------------------------------------------------------------

## 2. User Roles

-   **Student:** Creates an account, manages a profile and
    questionnaire, discovers eligible users, receives/sends introduction
    requests, messages connections, and uses safety tools.
-   **Moderator:** Reviews reports and evidence, records moderation
    decisions, and applies proportionate enforcement.
-   **Administrator:** Configures university domains, supported intents,
    questionnaire versions, match limits, rate limits, and policy
    settings. Administrative actions must be audited.
-   **System worker:** Generates recommendation batches, expires
    requests, sends notifications, and runs retention/deletion jobs.

Apply least-privilege access to every role.

------------------------------------------------------------------------

## 3. Product Modes

### 3.1 System Matches

The system delivers a small curated batch of recommendations on a
predictable schedule.

Initial configurable defaults: - New recommendations: 5 per user per
week. - Maximum unseen recommendations: 10. - Refresh cadence: weekly. -
Recently passed profiles: suppress for 90 days by default. - Batch size,
refresh cadence, and suppression period must be configuration values,
not hard-coded constants.

Each recommendation should display: - Profile information the candidate
has chosen to share. - Compatible connection intents. - A compatibility
score or qualitative score band, if enabled. - Evidence coverage /
confidence indicator. - Two or three grounded commonalities. - A
constructive difference or conversation starter where appropriate. -
Actions such as `Interested`, `Pass`, and optionally `Save for later`.

A person not shown in a batch must not be described as incompatible.
Batch capacity and ranking are not the same as compatibility.

Batch generation should apply eligibility checks, calculate
compatibility, and select a diverse set of candidates rather than simply
taking the top N scores. A batch should be reproducible using a recorded
algorithm version and selection seed.

### 3.2 Custom Discovery

Custom discovery lets users filter and explore eligible candidates
without an infinite feed.

Required behavior: - Compatibility is the default sort. - Support
filters for connection intent, interests, social preferences, and
relevant practical constraints. - Support pagination or explicit
`Show more`. - Apply eligibility, privacy, block, and safety checks on
every page. - Allow profile viewing and limited introduction requests. -
Do not sort by popularity, likes received, profile views, response rate,
or message volume. - Do not reveal hidden preferences or sensitive
questionnaire answers through filters or result counts. - Keep search
exposure and introduction-request quotas separate from system-match
quotas.

Ranking should primarily reflect compatibility. Among candidates with
sufficiently similar scores, use documented, bounded exposure balancing
or tie-breaking to avoid permanently privileging the same accounts. Do
not artificially reduce scores based on sensitive demographic
attributes.

### 3.3 Connections and Messaging

Use introduction requests rather than unrestricted messages to
strangers.

Initial configurable defaults: - Maximum pending introduction requests
sent per user: 3. - Maximum new requests received per user per rolling 7
days: 5. - Pending request expiry: 7 days. - A sender cannot create
duplicate active requests to the same recipient. - A declined request
cannot be repeatedly resent. - A block terminates the ability to
discover, request, or message the blocked account. - Do not show
recipients' request counts or popularity counters. - Do not reveal
whether a request was ignored, declined, or unavailable due to capacity;
present a neutral outcome to the sender.

A recipient's acceptance opens a conversation. Unaccepted requests must
not permit unrestricted messaging. Either participant can end a
conversation, block the other user, or report misconduct.

All limits must be configurable and tested with a university pilot.

------------------------------------------------------------------------

## 4. University Email Verification and Account Eligibility

### 4.1 Registration flow

1.  User enters a university email address and authentication
    credentials, or uses an approved identity provider.
2.  System sends a time-limited verification link or code.
3.  Server validates the verification token and permitted email domain.
4.  User completes required profile fields and the onboarding
    questionnaire.
5.  User explicitly opts into discoverability before appearing in search
    or recommendation batches.

### 4.2 Requirements

-   Maintain an administrator-managed allowlist of university email
    domains.
-   Validate email domains server-side.
-   Normalize email addresses consistently and enforce uniqueness.
-   Store authentication data separately from public profile data.
-   Never return university email addresses in public-profile, search,
    match, or analytics APIs.
-   Rate-limit verification and recovery attempts.
-   Avoid account-enumeration vulnerabilities.
-   Support email changes, account recovery, deactivation, and deletion.
-   Define policy for alumni, graduates, suspended accounts, and revoked
    university email access.
-   Do not assume that a university email domain alone proves current
    enrollment if alumni or affiliates can use it.
-   Use a mature authentication provider or established authentication
    framework instead of implementing password storage and recovery from
    scratch.

### 4.3 Account lifecycle

Suggested states: - `pending_verification` - `active` - `paused` -
`suspended` - `deactivated` - `deleted` (after applicable retention and
deletion requirements are satisfied)

Rules: - Only verified, active, discoverable users are eligible for
matching. - Paused users are excluded from new discovery and
recommendations. - Suspended users cannot send requests or messages. -
Deactivation immediately hides a profile and prevents new contact. -
Deletion follows a documented process that accounts for legal
obligations and necessary moderation evidence.

------------------------------------------------------------------------

## 5. Profile Requirements

### 5.1 Profile fields

Required or optional fields should be configurable.

-   Preferred/display name
-   Age or age range, subject to policy and privacy review
-   University verification badge (do not show email address)
-   Program/faculty (optional)
-   Connection intents
-   Interests and activities
-   Optional short biography
-   Social and communication preferences, in user-selected summaries
-   Ideal activities or first-meetup preferences
-   Optional conversation starter
-   Optional photos subject to privacy and moderation controls
-   Optional coarse availability; never show a live schedule

Do not require disclosure of gender, sexual orientation, religion,
political views, disability, or other sensitive traits. Where a
connection type requires a user to express a preference, collect only
what is necessary, explain its use, and let the user control visibility.

### 5.2 Data visibility classes

Every profile/questionnaire field must have a declared visibility
classification:

1.  **Public profile:** Explicitly selected by the user for display.
2.  **Matching-only:** Used by the matching engine but not shown
    verbatim.
3.  **Sensitive/private:** Optional, access-restricted, excluded from
    explanations unless the user explicitly chooses to disclose it.

The explanation generator must not leak private answers. For example,
display "You both enjoy reflective conversations," not another person's
exact answer to a private question.

------------------------------------------------------------------------

## 6. Compatibility Questionnaire

### 6.1 Onboarding

Start with approximately 15--25 onboarding questions and offer optional
deeper questions later. Do not require a long questionnaire before
showing value.

Question categories: - Interests and activities - Values and
priorities - Social and communication preferences - Lifestyle and
availability - Connection intent and expectations

Use varied answer types: - Multi-select interests - Importance/intensity
scales - Paired statements - Value rankings - Situational choices -
Preference ranges - Optional open-ended prompts

### 6.2 Question design requirements

-   Include `not sure`, `depends`, and `prefer not to answer` where
    appropriate.
-   Distinguish a hard deal-breaker from a soft preference.
-   Allow users to indicate how important an answer is.
-   Allow answer updates at any time.
-   Version questions and scoring rules.
-   Do not infer sensitive characteristics from hobbies, photos, writing
    style, or answer patterns.
-   Use open-ended answers as profile/conversation content only when
    explicitly shared; do not use opaque language-based psychological
    inference for scoring.
-   Store stable question identifiers and validated structured answers.

### 6.3 Suggested answer model

Each question should define: - Stable question ID - Version - Category -
Answer type - Allowed values / validation schema - Privacy class -
Whether the answer is optional - Whether importance can be configured -
Scoring configuration - Explanation templates allowed for that question

Questionnaire changes must not silently alter historical score
calculations. The engine must know which question and scoring versions
were used for each match.

------------------------------------------------------------------------

## 7. Compatibility Engine

### 7.1 Pipeline

For each viewer and candidate:

1.  Verify account and university eligibility.
2.  Check both users are active and discoverable.
3.  Check supported connection-intent compatibility.
4.  Apply explicit hard constraints.
5.  Apply blocks, moderation restrictions, and other safety exclusions.
6.  Calculate category compatibility scores.
7.  Calculate evidence coverage.
8.  Rank eligible candidates.
9.  Diversify system-match batches where appropriate.
10. Generate grounded explanations.
11. Recheck eligibility and safety immediately before returning a
    profile or delivering a message.

A high score must never override a block, safety restriction, hard
constraint, or user's decision not to be discoverable.

### 7.2 Initial category weights

Use configurable weights that sum to 1.0:

  Category                                 Initial weight
  -------------------------------------- ----------------
  Interests and activities                           0.25
  Values and priorities                              0.30
  Social and communication preferences               0.20
  Lifestyle and availability                         0.15
  Connection intent and expectations                 0.10
  **Total**                                      **1.00**

These weights are hypotheses to validate in a pilot, not scientific
facts. Connection intent should also be an eligibility constraint when
the selected intents are incompatible; a high average score must not
compensate for incompatible intent.

### 7.3 Score calculation

For a viewer `u` and candidate `v`, calculate each category score
`C_k(u, v)` on a scale from 0 to 100.

``` text
compatibility_score(u, v) =
    sum(weight[k] * category_score[k](u, v))
```

The final score is in the range 0--100.

Requirements: - Use question-specific compatibility matrices and
normalized distance/overlap functions. - Shared interests can contribute
positively without penalizing every different interest. - Values may
require agreement or complementary preferences depending on the
question. - Social preference scoring must account for preference
strength and flexibility. - Lifestyle scoring should assess practical
compatibility without exposing detailed schedules. - Hard constraints
are evaluated separately from weighted scoring. - Avoid claims that the
score represents a probability of relationship success, friendship,
attraction, or future behavior.

### 7.4 Missing answers and evidence coverage

Missing answers are not disagreements.

Calculate an evidence coverage value separately from compatibility:

``` text
evidence_coverage(u, v) =
    sum(category_weight[k] * jointly_answered_fraction[k])
    / sum(category_weight[k])
```

`jointly_answered_fraction[k]` is the proportion of relevant questions
in category `k` answered by both users with usable data, in the range
0--1.

Keep these concepts separate: - `compatibility_score`: alignment among
usable answers. - `evidence_coverage`: how much relevant information
supports the comparison.

Do not automatically penalize a person for an incomplete profile. Show a
neutral explanation such as "Strong alignment in the areas you've both
answered." Do not imply that a low coverage value means a person is a
poor match.

### 7.5 Symmetry and directional constraints

Use a symmetric pair score where category compatibility is symmetric.
Where a category is genuinely directional, calculate directional values
explicitly and document the rule. If an aggregate symmetric score is
useful:

``` text
pair_score(u, v) = (score(u, v) + score(v, u)) / 2
```

This averaging must not override hard constraints. Eligibility and
safety are checked independently for both users.

### 7.6 Explainability

Each recommendation should include two or three grounded explanations
based on answers that are approved for sharing.

Examples: - "You both enjoy hiking and exploring new places." - "You
have similar preferences for small-group activities." - "You both value
curiosity and personal growth." - "You may bring different perspectives
to planning activities."

Do not expose private answer text, infer sensitive traits, or generate
unsupported claims. Explanation templates should be tied to the
underlying question/category and validated scoring rules. Use
deterministic templates for the MVP rather than an unconstrained
generative explanation system.

### 7.7 Ranking and tie-breaking

Do not use these as matching features: - Likes or interest received -
Profile views - Message volume - Response rate - Perceived
attractiveness - Popularity - Engagement time

Use compatibility as the primary sort. When scores are sufficiently
close, use a documented, bounded tie-breaker or exposure balancing. A
deterministic pseudorandom tie-breaker can use a recorded rotating
weekly seed so ordering is reproducible within a batch without
permanently privileging the same accounts.

### 7.8 System batch diversification

Do not simply take the top N individual scores. Choose a batch that
maintains high compatibility while avoiding redundant recommendations.

A conceptual objective:

``` text
batch_utility(B, viewer) =
    sum(compatibility(viewer, candidate) for candidate in B)
    - diversity_penalty(B) * diversity_weight
```

Select up to the configured batch size, subject to eligibility and
exposure rules. An MVP greedy algorithm is sufficient: repeatedly choose
the eligible candidate with the highest score after subtracting a modest
redundancy penalty.

The redundancy penalty may use non-sensitive shared interests and other
appropriate profile dimensions. Do not force diversity using sensitive
identity attributes or lower an individual's compatibility score based
on demographics.

------------------------------------------------------------------------

## 8. Matching and Discovery Behavior

### 8.1 Recommendation lifecycle

Each recommendation should track: - Batch and algorithm version - Viewer
and candidate - Compatibility score - Evidence coverage - Category
scores, or a versioned reproducible reference to them - Explanation
data - Timestamp shown - Decision (`interested`, `passed`, `saved`,
`unseen`, or equivalent) - Exposure source (`system_match` or
`custom_discovery`)

Do not assume that a candidate not included in the current batch is
incompatible.

### 8.2 Exposure balancing

Store exposure and decisions separately from compatibility. This allows
the team to determine whether an account is rarely shown because of a
low compatibility score, limited activity, or batch-selection behavior.

Do not use exposure-balancing logic to manipulate scores. Document and
test it. Respect user-chosen filters and compatibility criteria.

### 8.3 Custom discovery pagination

-   Default sort: compatibility.
-   Use cursor-based pagination where practical.
-   Reapply eligibility and block checks on every page.
-   Avoid leaking sensitive information through filter counts.
-   Prevent users from bypassing contact limits by changing filters or
    page cursors.
-   Rate-limit high-volume profile enumeration.
-   Do not expose hidden account status, moderation flags, or private
    questionnaire data.

------------------------------------------------------------------------

## 9. Real-World Connection Features

The app should encourage, but never require, meeting in person.

Optional features: - Shared-activity suggestions derived from mutual,
shareable interests. - Conversation starters grounded in profile
information. - Meeting planner where both users propose a time window
and public campus location. - Campus clubs, public lectures, study
sessions, hobby groups, and small group activities. - Optional
post-interaction feedback about comfort and connection quality. - Easy
disengagement without requiring an explanation.

Do not reward users for reporting successful dates/meetings. Do not
require meetings to unlock features. Always include a reminder to choose
a public, comfortable setting and respect personal boundaries.

------------------------------------------------------------------------

## 10. Safety, Reporting, Blocking, and Moderation

These are MVP requirements.

### 10.1 User controls

Every profile and conversation must provide: - Report user - Block
user - Unmatch/end connection - Mute notifications - Leave
conversation - Restrict incoming introduction requests where supported

Report reasons should include: - Harassment - Unwanted sexual content -
Threats - Impersonation - Spam - Discrimination - Suspected underage
participation where relevant to product policy - Other misconduct

Allow an optional description and evidence attachment/reference. Explain
what information will be sent to moderators.

### 10.2 Reporting workflow

1.  Record the report, reason, timestamp, and evidence references.
2.  Apply immediate contact restrictions where needed.
3.  Flag urgent threats for prompt human review.
4.  Allow authorized moderators to review evidence and relevant prior
    enforcement history.
5.  Record a proportionate decision: no action, warning, messaging
    restriction, temporary suspension, or permanent removal.
6.  Notify the reporting user as appropriate without disclosing private
    enforcement details.
7.  Support appeals where appropriate.

### 10.3 Safety invariants

-   Never reveal the reporter's identity to the reported user.
-   Reporting must not reduce the reporter's score or visibility.
-   Blocks take effect immediately across search, recommendations,
    pending requests, and conversations.
-   Recheck blocks and account status at delivery time, not just when
    creating a match.
-   Limit moderator access to sensitive evidence and log administrative
    actions.
-   Define evidence retention and deletion rules.
-   Prevent repeated abusive reports from being used to manipulate
    matching rankings.
-   Clearly state that platform moderation is not an emergency service.

Obtain qualified privacy/legal review for the university and
jurisdiction before launch.

------------------------------------------------------------------------

## 11. Technical Architecture

### 11.1 Architecture approach

Use a modular monolith for the initial release. Keep modules separated
by responsibility but deploy a small number of services.

Suggested logical modules: 1. `auth`: registration, verification,
recovery, account lifecycle. 2. `profiles`: public profile fields,
visibility, profile preferences. 3. `questionnaires`: question versions,
answers, importance, privacy classification. 4. `matching`: eligibility,
pair scoring, explanations, ranking, batch selection. 5. `discovery`:
filters, pagination, result ordering, exposure controls. 6.
`connections`: introduction requests, mutual consent, conversation
permissions. 7. `messaging`: conversation membership, message delivery,
rate limits. 8. `safety`: blocks, reports, moderation decisions,
enforcement. 9. `notifications`: rate-limited delivery and
deduplication. 10. `admin`: university settings, limits, moderation,
aggregate metrics.

### 11.2 Suggested stack

These are recommendations, not mandatory dependencies: - Frontend:
Next.js + TypeScript, or an equivalent modern web framework. - Backend:
NestJS/Fastify with TypeScript, or FastAPI with Python. - Database:
PostgreSQL. - Background jobs: PostgreSQL-backed queue initially, or
Redis-backed queue if justified. - Authentication: managed OIDC provider
or established authentication library. - Media: private object storage
with controlled image delivery. - Deployment: one application service,
database, and background worker. - Monitoring: structured logs, error
tracking, uptime monitoring, and security audit logs.

PostgreSQL is sufficient for an initial single-university population.
Start with indexed relational queries and batch scoring. Do not add a
vector database, microservices, or ML infrastructure until scale or a
concrete use case justifies it.

------------------------------------------------------------------------

## 12. Database Schema

The following is the logical schema. Adapt naming and normalization to
the selected framework, but preserve the privacy and integrity
requirements.

  -------------------------------------------------------------------------
  Table                     Important fields        Purpose
  ------------------------- ----------------------- -----------------------
  `universities`            `id`, `name`,           University
                            `allowed_domains`,      configuration
                            `policy_config`         

  `users`                   `id`,                   Authentication and
                            `email_normalized`,     eligibility
                            `email_verified_at`,    
                            `university_id`,        
                            `account_status`,       
                            `created_at`            

  `profiles`                `user_id`,              Public profile
                            `display_name`, `bio`,  
                            `profile_visibility`,   
                            `discoverable_at`,      
                            `updated_at`            

  `questions`               `id`, `version`,        Versioned questionnaire
                            `category`,             
                            `answer_type`,          
                            `scoring_config`,       
                            `privacy_class`         

  `user_answers`            `user_id`,              Matching and profile
                            `question_id`,          answers
                            `answer_value`,         
                            `importance`,           
                            `updated_at`            

  `user_intents`            `user_id`,              Connection intentions
                            `intent_type`,          
                            `seeking_status`        

  `user_preferences`        `user_id`,              Preferences and
                            `preference_type`,      deal-breakers
                            `preference_value`,     
                            `is_hard_constraint`    

  `match_batches`           `id`, `user_id`,        Batch lifecycle and
                            `period_start`,         reproducibility
                            `status`,               
                            `algorithm_version`,    
                            `selection_seed`        

  `match_batch_items`       `batch_id`,             Recommendations
                            `candidate_user_id`,    
                            `score`,                
                            `evidence_coverage`,    
                            `explanation_data`,     
                            `decision_status`       

  `user_exposures`          `viewer_id`,            Exposure and
                            `candidate_id`,         repeat-display controls
                            `surface`,              
                            `last_shown_at`,        
                            `decision`              

  `introduction_requests`   `id`, `sender_id`,      Consent and quotas
                            `recipient_id`,         
                            `status`, `created_at`, 
                            `expires_at`            

  `conversations`           `id`, `created_at`,     Conversation lifecycle
                            `status`                

  `conversation_members`    `conversation_id`,      Conversation
                            `user_id`, `joined_at`  authorization

  `messages`                `id`,                   Message history
                            `conversation_id`,      
                            `sender_id`, `body`,    
                            `created_at`,           
                            `deleted_at`            

  `blocks`                  `blocker_id`,           Block relationships
                            `blocked_id`,           
                            `created_at`            

  `reports`                 `id`, `reporter_id`,    Misconduct reports
                            `reported_user_id`,     
                            `reason`, `status`,     
                            `created_at`            

  `moderation_actions`      `id`, `report_id`,      Enforcement history
                            `moderator_id`,         
                            `action_type`,          
                            `reason`, `created_at`  

  `audit_events`            `id`, `actor_id`,       Security/admin auditing
                            `event_type`,           
                            `target_type`,          
                            `target_id`,            
                            `created_at`            
  -------------------------------------------------------------------------

### 12.1 Integrity, indexing, and privacy

-   Enforce unique normalized email addresses within the correct account
    scope.
-   Use foreign keys for account, questionnaire, and connection
    relationships.
-   Enforce unique block pairs with clear directional semantics.
-   Prevent duplicate active introduction requests for a
    sender-recipient pair.
-   Enforce unique recommendation entries per batch and candidate.
-   Index university, account status, discoverability, supported
    filters, and conversation message order.
-   Enforce quotas transactionally.
-   Keep authentication email in a restricted store; consider a
    protected fingerprint for uniqueness if architecture requires it.
-   Do not make email addresses searchable by ordinary application
    roles.
-   Use structured, validated questionnaire answers rather than
    arbitrary unvalidated JSON.
-   JSONB may be used for versioned scoring configuration and
    explanation metadata.
-   Define retention and deletion policies for answers, messages,
    reports, and authentication records.

------------------------------------------------------------------------

## 13. API Specification

Use versioned REST endpoints for the MVP. These routes define intended
behavior and are not a complete OpenAPI document.

  Method    Endpoint                                   Purpose
  --------- ------------------------------------------ -------------------------------------
  `POST`    `/v1/auth/register`                        Begin registration
  `POST`    `/v1/auth/verify-email`                    Verify email token
  `GET`     `/v1/me`                                   Get current account/profile
  `PATCH`   `/v1/me/profile`                           Update profile
  `GET`     `/v1/questionnaires/current`               Get active questionnaire
  `PUT`     `/v1/me/answers`                           Save questionnaire answers
  `GET`     `/v1/matches/current`                      Get current recommendation batch
  `POST`    `/v1/matches/{id}/decision`                Interested, pass, or save
  `GET`     `/v1/discovery`                            Filter/rank candidates
  `POST`    `/v1/introduction-requests`                Send introduction request
  `POST`    `/v1/introduction-requests/{id}/accept`    Accept request
  `POST`    `/v1/introduction-requests/{id}/decline`   Decline request
  `GET`     `/v1/conversations`                        Get authorized conversations
  `POST`    `/v1/conversations/{id}/messages`          Send a message
  `POST`    `/v1/users/{id}/block`                     Block user
  `POST`    `/v1/reports`                              Submit report
  `GET`     `/v1/me/privacy`                           Get privacy/data-management options
  `POST`    `/v1/me/deactivation`                      Begin deactivation

### 13.1 Example discovery request

``` http
GET /v1/discovery?intent=friendship&sort=compatibility&limit=10
Authorization: Bearer <access_token>
```

### 13.2 Example response shape

``` json
{
  "items": [
    {
      "user_id": "opaque-user-id",
      "profile": {
        "display_name": "Alex",
        "interests": ["hiking", "science", "board games"]
      },
      "compatibility": {
        "score": 84,
        "evidence_coverage": 0.78,
        "reasons": [
          {
            "category": "interests",
            "label": "Shared interest in hiking"
          },
          {
            "category": "social_style",
            "label": "Similar preference for small gatherings"
          }
        ]
      },
      "actions": {
        "can_request_introduction": true
      }
    }
  ],
  "next_cursor": "opaque-pagination-cursor"
}
```

All example values are illustrative. Never return raw private
questionnaire answers, university emails, internal moderation flags, or
another user's hidden interest in the viewer.

### 13.3 API security

-   Enforce authorization on every endpoint.
-   Recheck eligibility, discoverability, blocks, and consent
    immediately before delivery.
-   Use idempotency keys for retriable state-changing operations.
-   Apply per-user and per-IP rate limits, especially to authentication
    and messaging.
-   Use opaque public IDs and prevent profile enumeration.
-   Ensure pagination cannot bypass eligibility checks or quotas.
-   Keep sensitive answer data and message bodies out of general
    application logs.
-   Validate all input on the server.
-   Use secure cookie/session/token handling and standard CSRF
    protections where applicable.

------------------------------------------------------------------------

## 14. State Machines and Transactional Rules

### 14.1 Introduction request states

States: - `pending` - `accepted` - `declined` - `expired` -
`cancelled` - `terminated` (for applicable safety/account events)

Rules: - Acceptance creates or enables a conversation. - Decline and
expiry close the request. - A pending request cannot be duplicated for
the same sender/recipient pair. - Blocks or suspensions prevent further
interaction regardless of request state. - Do not permit unrestricted
messages before acceptance. - Store timestamps for all state
transitions.

### 14.2 Concurrency

Quota checks must be atomic. Do not perform a read-count followed by an
independent insert without transaction protection. Concurrent requests
could otherwise exceed limits.

Use database transactions, constraints, or an atomic quota ledger for
introduction and message limits. Revalidate conversation membership,
account status, and block status when writing each message.

### 14.3 Auditability

For every generated batch, retain sufficient non-sensitive metadata to
reproduce or explain selection: - Algorithm version - Question/scoring
versions - Relevant configuration version - Selection seed - Candidate
score and evidence coverage - Explanation template IDs -
Eligibility/exclusion reason codes where safe to retain

Do not store unnecessary copies of sensitive answers in audit logs.

------------------------------------------------------------------------

## 15. Fairness and Algorithm Governance

No matching system can guarantee zero bias just by omitting demographic
fields. Proxy effects, missing data, uneven participation, and repeated
exposure can still create systematic disadvantages.

Requirements: 1. Do not create popularity feedback loops. 2. Separate
compatibility scores from visibility/exposure decisions. 3. Audit
exposure and outcomes for systematic differences where lawful, ethical,
and supported by appropriate consent. 4. Do not use sensitive
demographic data as an unreviewed ranking input. 5. Make the
questionnaire accessible to different communication styles,
disabilities, and language proficiency levels. 6. Do not reward long or
polished answers simply because they are longer. 7. Keep recommendation
explanations traceable to input data and versioned scoring rules. 8. Let
users correct answers, pause discovery, change intents, and reset
questionnaire responses. 9. Test with synthetic profiles and adversarial
cases before launch. 10. Review whether incomplete profiles or low
activity cause users to become effectively invisible. 11. Document
scoring weights, tie-breakers, exposure balancing, and changes. 12. Do
not claim that compatibility scores predict relationship success.

Demographic fairness is not the same as forcing equal visibility for all
profiles. Respect user-selected compatibility criteria while auditing
for avoidable, systematic exclusion.

------------------------------------------------------------------------

## 16. Analytics and Success Metrics

### 16.1 North-star metric

**Weekly meaningful connection rate:** the proportion of active users
who establish at least one mutually welcomed connection in a week and
voluntarily report that it was worthwhile.

Report this alongside active-user counts and survey response rates to
avoid misleading results from a small, highly engaged respondent group.

### 16.2 Supporting metrics

-   Questionnaire completion rate
-   Time to first relevant recommendation
-   Recommendation acceptance rate
-   Mutual connection rate
-   Introduction response rate
-   Voluntary meeting/activity rate
-   Unwanted contact reports
-   Report and block rates
-   Exposure distribution across eligible profiles
-   User-reported connection quality
-   User-reported safety and comfort
-   Account pause/deactivation reasons, where voluntarily supplied

Do not optimize solely for acceptance rate. A system that recommends
only obvious similarities may increase acceptance while reducing
meaningful discovery. A low meeting rate also does not automatically
indicate poor matching.

### 16.3 Analytics privacy

-   Prefer aggregate metrics.
-   Do not build advertising profiles or sell matching data.
-   Do not use message contents for general analytics.
-   Restrict sensitive individual-level data.
-   Define retention and access policies for all analytics datasets.

------------------------------------------------------------------------

## 17. MVP Delivery Plan

### Phase 1 --- Foundation

-   University email registration and verification
-   Account lifecycle and discoverability controls
-   Profile creation/editing
-   Versioned questionnaire and answer storage
-   Explainable weighted compatibility engine
-   Curated weekly recommendation batches
-   Privacy classification and API authorization

### Phase 2 --- Connections and Safety

-   Introduction requests and mutual acceptance
-   Conversations and messaging
-   Transactional request quotas
-   Blocking, reporting, moderation tooling, and audit events
-   Account deactivation and retention workflows
-   Abuse/rate-limit protections

### Phase 3 --- Custom Discovery

-   Filters and compatibility ranking
-   Cursor pagination
-   Exposure tracking and balancing
-   Search enumeration protections
-   Separate custom-discovery quotas

### Phase 4 --- Real-World Connection

-   Shared activity suggestions
-   Conversation starters
-   Optional meeting planner
-   Campus events and group activities
-   Voluntary feedback and fairness evaluation

Reporting, blocking, and core moderation are mandatory for the first
public release. The backend should support system matches and custom
discovery from the beginning even if custom discovery launches later.

------------------------------------------------------------------------

## 18. Acceptance Criteria

Before launch, verify all of the following:

-   [ ] Unverified accounts cannot become discoverable.
-   [ ] Users can opt out of discovery without deleting accounts.
-   [ ] Ineligible or blocked candidates never appear in recommendations
    or search.
-   [ ] Recommendation explanations use actual, shareable answer data.
-   [ ] Missing answers are not treated as disagreement.
-   [ ] Popularity and message volume do not affect compatibility
    scores.
-   [ ] System recommendations respect configured batch limits.
-   [ ] Custom search respects filters, pagination, eligibility, and
    exposure controls.
-   [ ] Introduction quotas remain correct under concurrent requests.
-   [ ] Unaccepted requests do not allow unrestricted messaging.
-   [ ] Blocking immediately prevents further contact and profile
    exposure.
-   [ ] Reporting is accessible from profiles and conversations.
-   [ ] Moderators can review reports and record actions.
-   [ ] Public APIs do not expose email addresses or private
    questionnaire answers.
-   [ ] Deactivation hides profiles and prevents new contact.
-   [ ] Deletion follows documented retention and legal requirements.
-   [ ] Recommendation batches and scoring rules are versioned and
    reproducible.
-   [ ] Automated tests cover authorization, concurrency, privacy, and
    abuse cases.
-   [ ] Users are not pressured or rewarded to meet in person.
-   [ ] Logs and analytics do not unnecessarily contain sensitive data.

------------------------------------------------------------------------

## 19. Recommended Initial Configuration

These values are hypotheses and must be configurable:

``` yaml
matching:
  system_batch_size: 5
  max_unseen_recommendations: 10
  refresh_period: weekly
  passed_profile_suppression_days: 90
  weights:
    interests: 0.25
    values: 0.30
    social_communication: 0.20
    lifestyle_availability: 0.15
    connection_intent_expectations: 0.10
  require_mutually_compatible_intent: true
  use_popularity_signals: false
  missing_answers_are_disagreement: false

discovery:
  default_sort: compatibility
  use_pagination: true
  enforce_eligibility_on_every_page: true
  enforce_blocks_on_every_page: true

introduction_requests:
  max_pending_sent: 3
  max_received_per_rolling_7_days: 5
  expiry_days: 7
  require_acceptance_before_unrestricted_messaging: true

privacy:
  university_email_public: false
  private_answers_in_public_profile: false
  sensitive_answers_in_explanations: false
  discoverability_requires_user_opt_in: true

safety:
  blocking_is_immediate: true
  reporting_required_for_mvp: true
  moderation_actions_audited: true
```

Implementation note: configuration parsing must validate types, ranges,
and weight totals. Do not blindly load YAML into production without
schema validation.

------------------------------------------------------------------------

## 20. Instructions for GitHub Copilot / AI Coding Assistants

Use this document as the product and architecture source of truth.

When implementing features: 1. Inspect the existing repository,
conventions, and test setup before proposing changes. 2. Do not assume a
framework, language, or folder layout that has not been established by
the repository. 3. Implement small, reviewable increments rather than
generating the entire application in one pass. 4. Preserve the product
invariants in this specification, especially consent, eligibility,
privacy, blocking, quotas, and explainability. 5. Put compatibility
weights, limits, and retention settings in validated configuration
rather than scattering magic numbers through code. 6. Use migrations for
schema changes and include rollback considerations where supported. 7.
Add unit tests for score calculations and category scoring. 8. Add
integration tests for eligibility, introduction state transitions,
blocks, rate limits, and transactional concurrency. 9. Add authorization
and privacy tests proving that email addresses and private questionnaire
answers cannot leak. 10. Add test fixtures for missing answers, hard
constraints, incompatible intents, ties, blocks, suspended users, and
incomplete profiles. 11. Do not add machine learning, behavioral
popularity ranking, or sensitive-trait inference without explicit
product approval and a documented review. 12. Do not implement
frontend-only security checks. Enforce critical rules on the server and,
where applicable, in the database. 13. Do not claim tests pass unless
they have actually been run. 14. Document non-obvious scoring rules and
state transitions. 15. If a requirement is ambiguous or conflicts with
another requirement, identify the conflict and ask for clarification
rather than silently inventing product behavior.

### Suggested implementation order

1.  Repository and architecture review.
2.  Database schema and migrations.
3.  Authentication, university verification, and account lifecycle.
4.  Profile visibility and questionnaire versioning.
5.  Pure, deterministic compatibility scoring functions with unit tests.
6.  Candidate eligibility and block/safety filtering.
7.  Recommendation batch generation and reproducibility.
8.  Discovery endpoint and pagination.
9.  Introduction request state machine and transactional quotas.
10. Messaging authorization and safety controls.
11. Reporting/moderation workflow.
12. Notifications, observability, analytics, and retention jobs.
13. End-to-end acceptance tests and security review.

------------------------------------------------------------------------

## 21. Open Decisions Before Production

Confirm these decisions before treating the specification as final: -
Which connection types are enabled at launch? - Is custom discovery
available at launch or after the first pilot? - Which profile fields are
visible by default? - What age and eligibility policy applies? - Does a
system match require mutual interest before messaging, while custom
discovery uses introduction requests? - Are group activities a core
launch feature or a later enhancement? - What is the moderation staffing
and escalation model? - What are the exact retention and deletion
policies? - Which privacy and legal requirements apply to the university
and deployment jurisdiction? - Which frontend, backend, hosting, and
identity-provider choices fit the project team?

------------------------------------------------------------------------

## Final Product Recommendation

Launch with a transparent questionnaire-based matching engine, five
curated recommendations per week, mutual consent for conversations, and
strong safety controls. Add custom discovery with its own quotas once
the core experience is validated.

The product's defining value is helping students find people they
genuinely connect with---not turning human relationships into an endless
optimization game.
