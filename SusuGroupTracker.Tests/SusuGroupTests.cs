using SusuGroupTracker.Core;

namespace SusuGroupTracker.Tests;

public class SusuGroupTests
{
    private static SusuGroup Create(params (string Id, string Name)[] members)
    {
        var group = new SusuGroup(50m);
        foreach (var member in members) group.AddMember(member.Id, member.Name);
        return group;
    }

    [Fact]
    public void PartialPaymentIsTrackedAndCanBeCompleted()
    {
        var group = Create(("ama", "Ama"));
        group.RecordContribution("ama", 30m);
        Assert.Equal(20m, Assert.Single(group.GetReport().Members).Owes);
        group.RecordContribution("ama", 20m);
        Assert.Equal(0m, Assert.Single(group.GetReport().Members).Owes);
    }

    [Fact]
    public void WeekCannotCloseUntilEveryoneHasPaid()
    {
        var group = Create(("ama", "Ama"), ("kofi", "Kofi"));
        group.RecordContribution("ama", 50m);
        var result = group.CloseWeek();
        Assert.False(result.Closed);
        Assert.Null(result.Payout);
        Assert.Equal("Kofi owes 50.00 GHS", result.Message);
        Assert.Equal(1, group.CurrentWeek);
    }

    [Fact]
    public void SuccessfulClosePaysPotToMembersInJoinOrder()
    {
        var group = Create(("ama", "Ama"), ("kofi", "Kofi"), ("esi", "Esi"));
        foreach (var member in group.Members) group.RecordContribution(member.Id, 50m);
        var first = group.CloseWeek();
        Assert.Equal("ama", first.Payout!.MemberId);
        Assert.Equal(150m, first.Payout.Amount);
        foreach (var member in group.Members) group.RecordContribution(member.Id, 50m);
        Assert.Equal("kofi", group.CloseWeek().Payout!.MemberId);
    }

    [Fact]
    public void DuplicateMemberIdsAreRejectedCaseInsensitively()
    {
        var group = Create(("ama", "Ama"));
        Assert.Throws<InvalidOperationException>(() => group.AddMember("AMA", "Another Ama"));
    }

    [Fact]
    public void UnknownMemberAndInvalidAmountsAreRejected()
    {
        var group = Create(("ama", "Ama"));
        Assert.Throws<KeyNotFoundException>(() => group.RecordContribution("missing", 1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => group.RecordContribution("ama", 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => group.RecordContribution("ama", 50.001m));
        Assert.Throws<ArgumentOutOfRangeException>(() => group.RecordContribution("ama", 51m));
    }

    [Fact]
    public void MembersCannotBeAddedAfterContributionsStart()
    {
        var group = Create(("ama", "Ama"));
        group.RecordContribution("ama", 1m);
        Assert.Throws<InvalidOperationException>(() => group.AddMember("kofi", "Kofi"));
    }

    [Fact]
    public void WeeklyContributionMustBePositiveAndCentExact()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SusuGroup(0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SusuGroup(10.001m));
    }

    [Fact]
    public void ReportIncludesCurrentWeekBalancesAndPayoutHistory()
    {
        var group = Create(("ama", "Ama"), ("kofi", "Kofi"));
        foreach (var member in group.Members) group.RecordContribution(member.Id, 50m);
        group.CloseWeek();
        group.RecordContribution("ama", 25m);
        var report = group.GetReport();
        Assert.Equal(2, report.CurrentWeek);
        Assert.Single(report.PayoutHistory);
        Assert.Equal(25m, report.Members.Single(m => m.MemberId == "ama").Owes);
        Assert.Equal(50m, report.Members.Single(m => m.MemberId == "kofi").Owes);
    }
}
