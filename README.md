# Susu Group Tracker

A .NET 8 command-line program for tracking one rotating susu savings group. Contributions are fixed per member per week; partial payments are supported, and the pot is paid to members in join order only after everyone has paid in full.

## Requirements

- .NET 10 SDK (the projects target .NET 10)

## Run

From this directory:

```powershell
dotnet run --project SusuGroupTracker.Cli
```

Enter the weekly contribution as a GHS decimal amount, then add member IDs and names in payout order. Amounts use a period as the decimal separator. Commands are:

- `pay <member-id> <amount>` — record a full or partial contribution for the current week.
- `close` — close the week and pay the pot if all members have paid, or list outstanding balances.
- `report` — show current balances and the payout history.
- `help` and `exit`.

## Tests

```powershell
dotnet test SusuGroupTracker.slnx
```

The automated tests cover partial payments, blocked week closure, payout order and amount, duplicate and unknown members, invalid contribution amounts, membership timing, and reporting.
