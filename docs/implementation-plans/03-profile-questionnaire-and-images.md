# Phase 3: Profile, Questionnaire, and Images

## Goal
Deliver the profile-onboarding features that let a verified member describe their profile, choose connection intents, satisfy the MVP image requirement, and complete the questionnaire. The server remains authoritative for validation, privacy, and whether the profile is complete enough for discoverability.

## Prerequisites
- Phase 2 complete.
- Phase 1 persistence, configuration, and restricted administration foundations complete.
- Age policy and image-approval workflow remain deferred beyond MVP.

## Features

### Feature 1: Profile setup and connection preferences

**Outcome:** A member can maintain the profile information required by onboarding and select one or more connection intents that represent both what they seek and what they are willing to receive interests or introduction requests about.

**Implementation:**
1. Define or confirm the profile setup request/response contracts in `MeetCampus.Contracts`; return only fields the member is allowed to see or edit.
2. Implement server-side profile setup and update operations in the Profile feature. Validate required values and all lookup IDs against active server-managed data; never trust client-supplied labels or eligibility state.
3. Validate selected intents against server-managed intent records. Reject unknown, duplicate, or incompatible values.
4. Add the Profile feature typed client and localized, accessible onboarding UI for entering profile data and selecting intents.
5. Connect successful updates to the existing eligibility service so completion and contact eligibility reflect the saved state immediately.

**Acceptance criteria:**
- A member can save and update valid profile data and multiple selected intents.
- Invalid or disabled lookup values and policy combinations are rejected server-side with safe, useful validation results.
- API responses expose no university email, private questionnaire data, or server-only eligibility details.

### Feature 2: Profile image onboarding

**Outcome:** A member can upload and manage the profile image required for MVP discoverability.

**Implementation:**
1. Reuse the existing server-side image validation, optimization, and `S3ProfileImageStorage` flow; retain ownership checks and the configured Garage-compatible storage client.
2. Expose image upload, replacement, and removal behavior through the Profile feature API and a typed client. Do not put storage credentials or storage operations in the WebAssembly client.
3. Add localized, accessible image controls and clear upload/validation feedback to profile onboarding.
4. Represent image presence in the central eligibility evaluation. A successfully stored image satisfies the MVP prerequisite; do not introduce approval states or review workflow.

**Acceptance criteria:**
- Invalid images are rejected before storage; stored images follow the existing validation/optimization and ownership rules.
- A successful upload satisfies the image prerequisite; image approval is not required for MVP eligibility.
- Replacing or removing an image updates eligibility consistently, and storage failures do not falsely mark an image as present.

### Feature 3: Questionnaire administration

**Outcome:** Authorized administrators can maintain the initial questionnaire using the Phase 1 restricted administration boundary.

**Implementation:**
1. Provide an Administrator-only client page and protected server API for question definitions, fixed categories, answer validation rules, and scoring configuration, using the existing persistence and authorization/auditing foundations.
2. Validate question types, allowed answer shapes/options, category assignments, and scoring configuration before saving changes. Questionnaire data is used only for matching; question visibility and compatibility explanation templates are out of scope.
3. Preserve stable question IDs. Track question update timestamps as defined in Phase 1 so changes can identify responses that members should refresh; do not introduce questionnaire versioning in this phase.
4. Store question prompts and option labels as English and French database translations. The EF model requires translation tables and schema changes; migration files are managed separately and are not created in this phase.
5. Keep administration UI/API access restricted to Administrators and record required audit events. Preserve option IDs and reject removal of options referenced by saved answers.

**Acceptance criteria:**
- Unauthorized users cannot read or change questionnaire administration data.
- Invalid definitions, scoring settings, and unsupported category values cannot be saved.
- A changed question can be identified for response refresh using its update timestamp without invalidating the stable question ID.

### Feature 4: Member questionnaire responses

**Outcome:** A member can answer and update the active questionnaire with validated structured values.

**Implementation:**
1. Define shared request/status contracts using stable question IDs; accept only the answer shape allowed by each current question definition.
2. Implement server-side submission and update operations that validate question existence, active status, answer type, allowed options, and any required-answer rules against current definitions.
3. Store response revisions and submission timestamps using the Phase 1 data model, and identify responses that need refresh after a question definition changes.
4. Add the Questionnaire typed client and localized, accessible answer/edit UI. Render the active definitions and return validation/status information without returning raw private or matching-only answer values.
5. Ensure questionnaire completeness is calculated from valid current responses, not from client-submitted completion flags.

**Acceptance criteria:**
- Unknown, inactive, stale, or malformed question answers are rejected with actionable validation results.
- Members can update their own responses but cannot read or change another member's private responses.
- Missing or refresh-required answers make the questionnaire incomplete; they are not treated as disagreement by scoring.

### Feature 5: Onboarding completion and discoverability integration

**Outcome:** Profile setup, image presence, and valid questionnaire responses consistently feed the existing central eligibility decision.

**Implementation:**
1. Add or confirm the required profile and questionnaire completion checks in the central eligibility service; do not create a second client- or feature-specific eligibility authority.
2. Ensure profile, image, intent, or answer changes cause subsequent eligibility checks to use the latest persisted state.
3. Return only the privacy-safe onboarding/eligibility status needed by the client to guide the member to incomplete steps.
4. Verify downstream recommendation/contact entry points continue to use the shared eligibility service from Phase 2.

**Acceptance criteria:**
- Missing required profile data, an uploaded image, or valid current questionnaire answers prevents discoverability.
- Completing all required onboarding steps makes the member eligible only when all other Phase 2 eligibility requirements are satisfied.
- Eligibility changes take effect after a relevant update without duplicating eligibility logic in the client.

## Privacy rules
- Never expose university email, raw private or exact matching-only answer values, or storage credentials in public contracts, client responses, logs, or explanations.
- Keep private and matching-only answer use within authorized server-side persistence and matching operations; enforce ownership for member answer operations.
- Do not return private answer values when rendering questionnaire status or an explanation.

## Cross-feature validation and phase exit criteria
- Profile and questionnaire API tests cover valid saves, invalid lookup IDs/intents/questions/answer shapes, ownership, and authorization.
- Image tests cover validation, optimization/storage outcomes, replacement/removal, ownership, and eligibility changes without requiring approval states.
- Questionnaire administration tests cover authorization, validation, stable IDs, update timestamps, and audit events.
- Eligibility tests cover each missing prerequisite and restoration after completion; missing answers never become score disagreement.
- Client onboarding text is localized and controls are accessible by keyboard and assistive technology.
- Build the affected projects and solution, then run relevant automated tests. No database migration files are created or changed as part of this phase; migration work is managed separately.
