# Decisions and assumptions

- Member IDs are trimmed and unique without regard to letter case. Names are required but need not be unique.
- Members are added in payout order and cannot be added after the first contribution is recorded. This keeps each payout equal to the fixed contribution multiplied by a stable group size.
- A week cannot close until every member's balance is zero. A blocked close leaves the week open so members can pay the outstanding amount.
- Payouts cycle through the join order and repeat if the group continues beyond one full rotation.
- GHS amounts use C# `decimal` and accept at most two decimal places. No floating-point arithmetic or rounding is used.
- Data is held in memory for the duration of the CLI session. It is not saved after exit.

## Improvements with more time

- Persist groups and payment history to a database or durable file.
- Add dates, receipts, audit events, and exportable reports.
- Add an integration test around the interactive command loop.
