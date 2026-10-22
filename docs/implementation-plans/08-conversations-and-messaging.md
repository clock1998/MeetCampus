# Phase 8: Conversations and Messaging

## Goal
Deliver basic text-only conversations for accepted mutual connections.

## Prerequisites
- Phase 7 complete.

## Increments
1. Define privacy-safe conversation/message contracts and typed client API client.
2. Implement authorized server APIs to list a participant’s conversations, retrieve ordered messages, send text, leave/end a connection, and mute notification state if notifications exist.
3. Recheck on every send that the sender is a participant, connection state is accepted/open, neither direction is blocked, and both accounts remain eligible for contact.
4. Persist messages with ordering indexes and bounded validation for text content; do not add attachments, reactions, typing indicators, read receipts, or content analytics.
5. Add routed Blazor conversation list and detail pages with localizable, accessible empty/error/send states.
6. Ensure ended/left/blocked conversations cannot be used to send new messages and are displayed only according to the participant’s permitted history policy.

## Tests and exit criteria
- Authorization tests reject non-participants, pending connections, blocked pairs, suspended/paused accounts, and ended connections.
- Ordering and message-validation tests cover concurrent sends and invalid content.
- UI tests cover authorized rendering and send-failure handling.
- Responses never disclose email, private questionnaire answers, object-storage credentials, or enforcement details.
