# Phase 6: Recommendation Decisions and Interest

## Goal
Present finite recommendations in the Blazor client and capture server-authorized pass, save, and interest decisions.

## Prerequisites
- Phase 5 complete.

## Increments
1. Add recommendation contracts and a typed WASM client for list/retrieval and decision operations.
2. Build a routed, authorized recommendation page that displays the finite current set rather than an endless/swipe interface.
3. Render a qualitative compatibility band, evidence coverage, two or three grounded commonalities, and an optional supported conversation starter.
4. Add accessible pass, save, interest, block, and report entry points. Persist decision changes server-side against the viewer’s batch item.
5. On pass, apply configured suppression and remove/mark the item appropriately without disclosing the candidate’s later state.
6. On interest, validate that the viewer and candidate remain eligible, in the recommendation relationship, and intent-compatible before creating/updating pending contact state.

## Privacy and UX rules
- Do not show exact scores or private matching data.
- Do not reveal whether someone passed, ignored, blocked, or became unavailable.
- Do not offer profile browsing, custom discovery, or pagination.

## Tests and exit criteria
- API tests prevent decisions on another viewer’s item, expired/non-current recommendation, blocked candidate, or ineligible account.
- UI/component tests cover empty, eligible, and decision-updated states.
- Recommendation content is localized, keyboard-accessible, and uses only contract data approved for display.
