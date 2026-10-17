using System.Globalization;
using SusuGroupTracker.Core;

static string ReadRequired(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();
        if (line is null) throw new EndOfStreamException();
        var value = line.Trim();
        if (!string.IsNullOrWhiteSpace(value)) return value;
        Console.WriteLine("Value cannot be empty.");
    }
}

static decimal ReadMoney(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();
        if (line is null) throw new EndOfStreamException();
        if (decimal.TryParse(line, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)) return value;
        Console.WriteLine("Enter a valid amount, for example 50.00.");
    }
}

decimal contribution;
try { contribution = ReadMoney("Fixed weekly contribution (GHS): "); }
catch (EndOfStreamException) { return; }
SusuGroup group;
try { group = new SusuGroup(contribution); }
catch (ArgumentException ex) { Console.WriteLine(ex.Message); return; }

Console.WriteLine("\nAdd members in payout order. Enter a blank ID when finished.");
while (true)
{
    Console.Write("Member ID (blank to finish): ");
    var line = Console.ReadLine();
    if (line is null) return;
    var id = line.Trim();
    if (string.IsNullOrEmpty(id)) break;
    var name = ReadRequired("Name: ");
    try { group.AddMember(id, name); Console.WriteLine("Member added."); }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { Console.WriteLine(ex.Message); }
}

if (group.Members.Count == 0) { Console.WriteLine("A group needs at least one member."); return; }
Console.WriteLine("\nCommands: pay <member-id> <amount>, close, report, help, exit");
while (true)
{
    Console.Write($"Week {group.CurrentWeek}> ");
    var line = Console.ReadLine();
    if (line is null) return;
    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (parts.Length == 0) continue;
    try
    {
        switch (parts[0].ToLowerInvariant())
        {
            case "pay" when parts.Length == 3 && decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount):
                group.RecordContribution(parts[1], amount);
                Console.WriteLine("Contribution recorded.");
                break;
            case "close":
                Console.WriteLine(group.CloseWeek().Message);
                break;
            case "report":
                var report = group.GetReport();
                Console.WriteLine($"Current week: {report.CurrentWeek}; weekly contribution: {report.WeeklyContribution:0.00} GHS");
                foreach (var balance in report.Members)
                    Console.WriteLine($"{balance.MemberName} ({balance.MemberId}): paid {balance.Paid:0.00}, owes {balance.Owes:0.00} GHS");
                Console.WriteLine("Payout history:");
                if (report.PayoutHistory.Count == 0) Console.WriteLine("  (none)");
                foreach (var payout in report.PayoutHistory) Console.WriteLine($"  Week {payout.Week}: {payout.MemberName} received {payout.Amount:0.00} GHS");
                break;
            case "help": Console.WriteLine("pay <member-id> <amount> | close | report | exit"); break;
            case "exit": return;
            default: Console.WriteLine("Invalid command. Type 'help' for available commands."); break;
        }
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException) { Console.WriteLine($"Error: {ex.Message}"); }
}
