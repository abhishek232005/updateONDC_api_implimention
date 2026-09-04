# FIS12 Personal Loan — Offline flow

The BAP creates a `search` with `ONDC:FIS12`, version `2.0.3`, `PERSONAL_LOAN`, BAP terms, and the offline-contract term. Its transaction ID remains fixed for the entire journey; every message has a new UUID message ID.

1. `search` is signed and sent to the configured gateway/network target.
2. A BPP calls `on_search`. The BAP verifies the BPP/gateway signature, validates context and catalog payload, then records `SearchReceived`.
3. `select` uses the chosen `order.provider.id` and item; its BPP URI is the HTTPS destination.
4. `on_select` contains the BPP draft order/xinput information and moves the transaction to `SelectReceived`.
5. `confirm` submits the selected item and, when the official flow requires it, the xinput form response. No automatic confirm retry is performed.
6. `on_confirm` carries the application/order confirmation and moves to `ConfirmReceived`.
7. `status` uses `message.ref_id` to request the current application status.
8. `on_status` is signature-verified and records the latest received status.

All callbacks must use the exact raw JSON bytes that were signed. The callback public key must be obtained from a trusted ONDC registry source and matched to `keyId`; a configuration map is provided only as a bootstrap/test resolver.
