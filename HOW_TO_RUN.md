# How to Run Susu Group Tracker

## Requirements

Install the .NET 10 SDK.

## Start the program

Open PowerShell in the repository folder and run:

```powershell
dotnet run --project .\SusuGroupTracker.Cli\SusuGroupTracker.Cli.csproj
```

The command runs **`SusuGroupTracker.Cli/Program.cs`**, which contains the command-line entry point. You do not need to open or run the `.cs` file directly.

When prompted, enter the fixed weekly contribution in GHS. Add each member's ID and name in payout order, then press Enter at the blank member ID prompt.

## Available commands

- `pay <member-id> <amount>` records a full or partial payment. Example: `pay ama 30.00`.
- `close` closes the week if everyone has paid, or reports who still owes money.
- `report` shows the current week balances and payout history.
- `help` lists the commands.
- `exit` quits the program.

Use a period as the decimal separator for money, such as `50.00`. Group data is held in memory and resets when the program exits.

## Run the tests

From the repository folder, run:

```powershell
dotnet test .\SusuGroupTracker.Tests\SusuGroupTracker.Tests.csproj
```

The first test run may need to restore test packages from NuGet.
