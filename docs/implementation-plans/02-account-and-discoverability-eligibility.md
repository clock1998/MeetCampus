# Phase 2: Account and Discoverability Eligibility

## Goal
Enforce university verification and make a single server-side eligibility service the authority for discoverability and contact eligibility.

## Prerequisites
- Phase 1 complete.
- Use dummy/no-op email delivery for MVP verification; production email and age policy remain deferred.

## Increments
1. Add allowed-domain lookup and normalized university-email registration/verification checks to the existing Identity account flow.
2. Implement non-enumerating verification and recovery responses with configurable rate limits.
3. Implement account lifecycle updates for pause, activation, suspension, and removal, enforcing them in protected operations.
4. Add an eligibility service that evaluates verification, lifecycle status, discoverability opt-in, required profile completion, uploaded profile image, questionnaire completeness, recipient capacity, blocks, and moderation restrictions.
5. Invoke the eligibility service wherever a profile can be recommended, contacted, or messaged; do not duplicate client-side eligibility decisions.
6. Return only a privacy-safe eligibility/onboarding status contract to the client.

## Tests and exit criteria
- Disallowed/unverified/duplicate normalized emails are rejected without account enumeration.
- Each failed prerequisite makes a user ineligible immediately.
- Re-enabling all prerequisites restores eligibility when no quota or moderation restriction applies.
- Public eligibility responses exclude email addresses and private questionnaire data.
