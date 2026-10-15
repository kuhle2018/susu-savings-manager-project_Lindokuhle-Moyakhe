using SusuSavingsManager.Core;
using Xunit;

namespace SusuSavingsManager.Tests;

public class SusuGroupTests
{
    [Fact]
    public void CreateGroup_RejectsNonPositiveContribution()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SusuGroup(0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SusuGroup(-10m));
    }

    [Fact]
    public void AddMember_RejectsDuplicateName()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");

        var exception = Assert.Throws<InvalidOperationException>(() => group.AddMember("Ama"));
        Assert.Contains("already in the group", exception.Message);
    }

    [Fact]
    public void RecordPayment_RejectsZeroOrNegativeAmount()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");

        Assert.Throws<ArgumentOutOfRangeException>(() => group.RecordPayment("Ama", 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => group.RecordPayment("Ama", -10m));
    }

    [Fact]
    public void RecordPayment_RejectsOverPaymentForWeek()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");
        group.RecordPayment("Ama", 30m);

        var exception = Assert.Throws<InvalidOperationException>(() => group.RecordPayment("Ama", 25m));
        Assert.Contains("exceeds what 'Ama' owes", exception.Message);
    }

    [Fact]
    public void PartialPayment_AllowsMemberToPayInInstallments()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");
        group.AddMember("Kofi");

        group.RecordPayment("Ama", 20m);

        Assert.Equal(20m, group.GetPaidAmount("Ama"));
        Assert.Equal(30m, group.GetOutstandingAmount("Ama"));

        var closeResult = group.CloseCurrentWeek();
        Assert.False(closeResult.CanClose);
        Assert.Contains("Kofi", closeResult.Message);
    }

    [Fact]
    public void CloseCurrentWeek_CannotCloseWhenAnyMemberStillOwes()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");
        group.AddMember("Kofi");
        group.AddMember("Esi");

        group.RecordPayment("Ama", 50m);
        group.RecordPayment("Kofi", 50m);
        group.RecordPayment("Esi", 30m);

        var result = group.CloseCurrentWeek();
        Assert.False(result.CanClose);
        Assert.Single(result.UnpaidMembers);
        Assert.Equal("Esi", result.UnpaidMembers[0].Name);
        Assert.Equal(20m, result.UnpaidMembers[0].AmountDue);
    }

    [Fact]
    public void CloseCurrentWeek_PaysPotAndAdvancesWeekWhenEveryMemberHasPaid()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");
        group.AddMember("Kofi");
        group.AddMember("Esi");

        foreach (var member in group.Members)
        {
            group.RecordPayment(member, 50m);
        }

        var result = group.CloseCurrentWeek();

        Assert.True(result.CanClose);
        Assert.Equal("Ama", result.Recipient);
        Assert.Equal(150m, result.TotalPaid);
        Assert.Equal(2, group.CurrentWeek);
        Assert.Equal("Ama", group.PayoutHistory[0].Recipient);
    }

    [Fact]
    public void PayoutOrder_RotatesToNextMemberAfterEachSuccessfulClose()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");
        group.AddMember("Kofi");
        group.AddMember("Esi");

        foreach (var member in group.Members)
        {
            group.RecordPayment(member, 50m);
        }

        var firstClose = group.CloseCurrentWeek();
        Assert.Equal("Ama", firstClose.Recipient);

        foreach (var member in group.Members)
        {
            group.RecordPayment(member, 50m);
        }

        var secondClose = group.CloseCurrentWeek();
        Assert.Equal("Kofi", secondClose.Recipient);
    }

    [Fact]
    public void Report_ListsCurrentWeekFullyPaidAndOwingMembers()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");
        group.AddMember("Kofi");

        group.RecordPayment("Ama", 50m);
        group.RecordPayment("Kofi", 30m);

        var report = group.GetReport();
        Assert.Contains("Current week: 1", report);
        Assert.Contains("Fully paid: Ama", report);
        Assert.Contains("Owing: Kofi owes GHS 20.00", report);
    }

    [Fact]
    public void AddMember_RejectsUnknownMemberNamesOnPayment()
    {
        var group = new SusuGroup(50m);
        group.AddMember("Ama");

        var exception = Assert.Throws<InvalidOperationException>(() => group.RecordPayment("Kofi", 50m));
        Assert.Contains("Unknown member 'Kofi'", exception.Message);
    }
}
