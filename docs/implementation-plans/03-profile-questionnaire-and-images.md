# Phase 3: Profile, Questionnaire, and Images

## Goal
Provide required onboarding data while protecting private answers and requiring an uploaded profile image for discoverability.

## Prerequisites
- Phase 2 complete.
- Age policy and image-approval workflow remain deferred beyond MVP.

## Increments
1. Refactor profile contracts, entities, setup service, and Blazor onboarding UI to capture only approved required/optional fields from section 4.3.
2. Support multiple connection intents and per-intent inbound contact restrictions; enforce server validation against enabled intent rules.
3. Preserve existing server-side image validation/optimization/storage. An uploaded image satisfies the MVP image prerequisite; defer image review/approval states and workflow.
4. Create questionnaire-version and question-definition administration, including category, validation, privacy class, scoring configuration, and approved explanation templates.
5. Add baseline questionnaire answer API/client UI using stable question IDs and validated structured values.
6. Compute profile/questionnaire completion through the central eligibility service; missing answers are incomplete for discoverability but never score as disagreement.

## Privacy rules
- Public profile and recommendation contracts contain only approved shareable fields.
- University email, private answers, exact matching-only answers, and storage credentials never leave server-side boundaries.
- Sensitive traits may be included as required fields when explicitly defined in approved profile/questionnaire configuration; classify and protect answers according to their configured privacy class.

## Tests and exit criteria
- Profile and questionnaire server validation rejects invalid lookup IDs, intents, questions, and answer shapes.
- An uploaded image is required for eligibility; image approval is not an MVP gate.
- Questionnaire-version rules preserve historical answers.
- Client onboarding text is localized and accessible.
