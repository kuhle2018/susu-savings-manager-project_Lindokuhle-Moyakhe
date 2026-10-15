# Susu Savings Manager

A .NET console application for tracking one rotating susu savings group. Members pay a fixed weekly contribution in Ghanaian cedis (GHS). When every member has paid for the week, the entire pot is paid to the next member in join order.

## Features

- Set a fixed weekly contribution and create a group.
- Add members in payout order.
- Record full or partial payments.
- See who has paid, who still owes, and how much is outstanding.
- Close a week only after all members have paid in full.
- Track the payout recipient and pot for each completed week.
- Reject duplicate or unknown members, non-positive payments, and payments greater than the member's outstanding balance.
- Handle money with C# `decimal` arithmetic and display amounts in GHS.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

Check that the SDK is installed:

```bash
dotnet --version
```

## Run the application

From the repository root, run:

```bash
dotnet run --project src/SusuSavingsManager.Cli/SusuSavingsManager.Cli.csproj
```

Use the menu to manage the group:

| Option | Action |
| --- | --- |
| 1 | Create a group and set its weekly contribution |
| 2 | Add a member |
| 3 | Record a member's payment |
| 4 | Close the current week |
| 5 | Show the current-week report and payout history |
| 6 | Exit |

Create the group and add members before recording a payment. Member order is payout order. Partial payments are accepted; a member can make another payment later in the same week to clear their balance. A week with outstanding balances cannot close.

## Example

For three members (Ama, Kofi, and Esi) contributing GHS 50.00 each:

1. Create the group with a weekly contribution of `50.00`.
2. Add Ama, Kofi, and Esi, in that order.
3. Record payments of `50.00` for Ama, `50.00` for Kofi, and `30.00` for Esi.
4. Choose **4. Close the current week**. The application reports that Esi owes GHS 20.00; no payout is made.
5. Record a further payment of `20.00` for Esi and close the week again.

Ama receives the GHS 150.00 pot. The application advances to week 2, where Kofi is next in payout order.

## Run the tests

From the repository root, run:

```bash
dotnet test tests/SusuSavingsManager.Tests/SusuSavingsManager.Tests.csproj
```

The automated tests cover partial payments, an unsuccessful week close, successful payouts, payout order, reports, and invalid input.

## Assumptions and limitations

- Member names are unique, ignoring differences in letter case.
- Members can be added before the first payment starts the group. The roster stays fixed once a payment has been recorded or a payout has been processed.
- Each member owes the same fixed contribution for each week; a partial payment carries no automatic credit into later weeks.
- The group and payment history are held in memory and are lost when the application exits.

See [NOTES.md](NOTES.md) for further decisions and potential improvements.
