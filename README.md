# Susu Savings Manager

A C# console application for managing one rotating susu savings group. Members contribute a fixed weekly amount in Ghanaian cedis (GHS); when everyone has paid, the full pot goes to the next member in join order.

## Requirements

- .NET 8 SDK or later.

## Run the application

```bash
dotnet run --project src/SusuSavingsManager.Cli
```

Follow the prompts to set the weekly contribution and add the initial members. The menu lets you record a member's payment, close the current week, and view the current balances and payout history. Entering a partial payment does not prevent the member from making additional payments during the same week.

## Run the tests

```bash
dotnet test tests/SusuSavingsManager.Tests
```

Tests cover payments, partial payments, unsuccessful and successful week closures, payout order and amounts, group completion, and invalid input.

## Assumptions

See [NOTES.md](NOTES.md) for the roster, payment precision, and payout-cycle rules, and for potential future improvements.
