# Phase 9: Blocks, Reports, and Moderation

## Goal
Provide immediate safety controls from recommendations and conversations, plus a restricted, auditable moderation workflow.

## Prerequisites
- Phase 8 complete.
- Moderator/administrator roles and restricted operational surface established in Phase 1.

## Increments
1. Implement a unique directional block service/API usable from recommendation profiles and conversations. Apply its effects immediately to discovery, contact eligibility, profile access, connection state, and message authorization in both relevant directions.
2. Add report creation with only specified reason values and optional text description; exclude evidence attachments.
3. Implement privacy-safe client actions for block, report, end/leave, and notification mute where notifications exist.
4. Implement moderator-only report retrieval with least-privilege access to report data and relevant enforcement history.
5. Implement auditable moderator actions: no action, warning, temporary messaging restriction, temporary suspension, and permanent removal. Enforce outcomes through lifecycle/eligibility services.
6. Notify reporters of resolution without revealing reporter identity to the reported user or exposing private enforcement details.
7. Add clear non-emergency-service wording at relevant report/help entry points.

## Tests and exit criteria
- Block tests prove immediate disappearance from recommendations/contact/profile access and no further messages.
- Report API tests validate reason values, authorization, and reporter-identity isolation.
- Moderator action tests verify authorization, audit entries, account enforcement, and privacy-safe notification.
- Reports never affect the reporter’s score or visibility.
