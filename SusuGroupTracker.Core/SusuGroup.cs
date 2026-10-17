namespace SusuGroupTracker.Core;

public sealed record Member(string Id, string Name);
public sealed record Payout(int Week, string MemberId, string MemberName, decimal Amount);
public sealed record MemberBalance(string MemberId, string MemberName, decimal Paid, decimal Owes);
public sealed record WeekCloseResult(bool Closed, string Message, Payout? Payout, IReadOnlyList<MemberBalance> Outstanding);
public sealed record GroupReport(int CurrentWeek, decimal WeeklyContribution, IReadOnlyList<MemberBalance> Members, IReadOnlyList<Payout> PayoutHistory);

/// <summary>In-memory tracker for one rotating savings group using exact decimal GHS values.</summary>
public sealed class SusuGroup
{
    private readonly List<Member> _members = [];
    private readonly Dictionary<int, Dictionary<string, decimal>> _payments = [];
    private readonly List<Payout> _payouts = [];

    public SusuGroup(decimal weeklyContribution)
    {
        if (weeklyContribution <= 0 || decimal.Round(weeklyContribution, 2) != weeklyContribution)
            throw new ArgumentOutOfRangeException(nameof(weeklyContribution), "Weekly contribution must be positive and have at most two decimal places.");
        WeeklyContribution = weeklyContribution;
    }

    public decimal WeeklyContribution { get; }
    public int CurrentWeek { get; private set; } = 1;
    public IReadOnlyList<Member> Members => _members.AsReadOnly();
    public IReadOnlyList<Payout> PayoutHistory => _payouts.AsReadOnly();

    public Member AddMember(string id, string name)
    {
        id = NormalizeRequired(id, nameof(id));
        name = NormalizeRequired(name, nameof(name));
        if (CurrentWeek != 1 || _payments.Count != 0 || _payouts.Count != 0)
            throw new InvalidOperationException("Members can only be added before the first contribution is recorded.");
        if (_members.Any(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A member with ID '{id}' already exists.");
        var member = new Member(id, name);
        _members.Add(member);
        return member;
    }

    public void RecordContribution(string memberId, decimal amount)
    {
        var member = FindMember(memberId);
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Contribution must be greater than zero.");
        if (decimal.Round(amount, 2) != amount) throw new ArgumentOutOfRangeException(nameof(amount), "Contribution must have at most two decimal places.");
        var payments = GetWeekPayments(CurrentWeek);
        var paid = payments.GetValueOrDefault(member.Id);
        var owed = WeeklyContribution - paid;
        if (amount > owed) throw new ArgumentOutOfRangeException(nameof(amount), $"Contribution exceeds the member's remaining balance of {owed:0.00} GHS.");
        payments[member.Id] = paid + amount;
    }

    public WeekCloseResult CloseWeek()
    {
        if (_members.Count == 0) throw new InvalidOperationException("Add at least one member before closing a week.");
        var outstanding = GetBalances(CurrentWeek).Where(b => b.Owes > 0).ToArray();
        if (outstanding.Length > 0)
            return new WeekCloseResult(false, string.Join("; ", outstanding.Select(b => $"{b.MemberName} owes {b.Owes:0.00} GHS")), null, outstanding);

        var recipient = _members[_payouts.Count % _members.Count];
        var payout = new Payout(CurrentWeek, recipient.Id, recipient.Name, WeeklyContribution * _members.Count);
        _payouts.Add(payout);
        CurrentWeek++;
        return new WeekCloseResult(true, $"Week {payout.Week} closed. {payout.MemberName} receives {payout.Amount:0.00} GHS.", payout, []);
    }

    public GroupReport GetReport() => new(CurrentWeek, WeeklyContribution, GetBalances(CurrentWeek), _payouts.ToArray());

    private List<MemberBalance> GetBalances(int week)
    {
        var payments = _payments.GetValueOrDefault(week);
        return _members.Select(m =>
        {
            var paid = payments?.GetValueOrDefault(m.Id) ?? 0m;
            return new MemberBalance(m.Id, m.Name, paid, WeeklyContribution - paid);
        }).ToList();
    }

    private Dictionary<string, decimal> GetWeekPayments(int week)
    {
        if (!_payments.TryGetValue(week, out var payments)) _payments[week] = payments = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        return payments;
    }

    private Member FindMember(string id)
    {
        id = NormalizeRequired(id, nameof(id));
        return _members.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Unknown member ID '{id}'.");
    }

    private static string NormalizeRequired(string value, string parameter)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be empty.", parameter);
        return value.Trim();
    }
}
