# Notes and assumptions

## Business assumptions

1. A susu group is created with a fixed weekly contribution in GHS and a roster of members in join order.
2. Members can only be added before a group starts its first week. This keeps the payout order stable and avoids changing an active cycle after the week has begun.
3. A contribution is recorded per member per current week. Partial payments are allowed, and a member may contribute multiple times until their weekly obligation is satisfied.
4. Each week closes only once all members have paid the full weekly amount. If any member still owes money, the system does not pay out and reports which members owe what.
5. The weekly pot is paid to the next member in join order after a successful week close. This advances the current week and the payout cursor.
6. Money is handled as exact decimal values, not floating-point values, so there are no rounding errors.

## Decisions made

- Duplicate names are rejected to protect each member's unique identity.
- A member cannot record a payment above their outstanding balance for the current week.
- Unknown members, non-positive amounts, and invalid week operations are treated as errors with clear messages.
- The current week count begins at 1 and increments after a successful close.
- Payout history stores successively paid recipients and the exact amount paid.

## Potential improvements

- Add persistence so the group can be saved and resumed.
- Add a string-based or numeric ID system for members if the group is larger than simple names.
- Support editing or removing members from a group that has not started.
- Add a richer report with CSV export and week-by-week summaries.
