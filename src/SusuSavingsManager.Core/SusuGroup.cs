using System.Globalization;

namespace SusuSavingsManager.Core;

public sealed class SusuGroup
{
    private readonly List<string> _members = new();
    private readonly Dictionary<string, decimal> _paymentsForCurrentWeek = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<PayoutRecord> _payoutHistory = new();
    private readonly HashSet<string> _memberSet = new(StringComparer.OrdinalIgnoreCase);
    private int _nextPayoutIndex;
    private bool _groupHasStarted;

    public SusuGroup(decimal weeklyContribution)
    {
        if (weeklyContribution <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(weeklyContribution), "Weekly contribution must be greater than zero.");
        }

        WeeklyContribution = weeklyContribution;
    }

    public decimal WeeklyContribution { get; }

    public int CurrentWeek { get; private set; } = 1;

    public IReadOnlyList<string> Members => _members;

    public IReadOnlyList<PayoutRecord> PayoutHistory => _payoutHistory;

    public void AddMember(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Member name is required.", nameof(name));
        }

        if (_groupHasStarted)
        {
            throw new InvalidOperationException("Members cannot be added once the week has started or a payout has been processed.");
        }

        if (_memberSet.Contains(name))
        {
            throw new InvalidOperationException($"Member '{name}' is already in the group.");
        }

        _members.Add(name);
        _memberSet.Add(name);
    }

    public void RecordPayment(string memberName, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            throw new ArgumentException("Member name is required.", nameof(memberName));
        }

        if (!_memberSet.Contains(memberName))
        {
            throw new InvalidOperationException($"Unknown member '{memberName}'.");
        }

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment must be greater than zero.");
        }

        var currentOwed = GetOutstandingAmount(memberName);
        if (amount > currentOwed)
        {
            throw new InvalidOperationException(
                $"Payment of {MoneyFormatter.Format(amount)} exceeds what '{memberName}' owes for week {CurrentWeek}.");
        }

        _groupHasStarted = true;
        _paymentsForCurrentWeek[memberName] = GetPaidAmount(memberName) + amount;
    }

    public WeekCloseResult CloseCurrentWeek()
    {
        var unpaidMembers = GetUnpaidMembers();
        if (unpaidMembers.Count > 0)
        {
            var payerSummary = unpaidMembers
                .Select(member => $"{member.Name} owes {MoneyFormatter.Format(member.AmountDue)}")
                .ToList();

            return new WeekCloseResult
            {
                CanClose = false,
                Message = $"Week {CurrentWeek} cannot close. {string.Join("; ", payerSummary)}.",
                UnpaidMembers = unpaidMembers
            };
        }

        if (_members.Count == 0)
        {
            return new WeekCloseResult
            {
                CanClose = false,
                Message = $"Week {CurrentWeek} cannot close because the group has no members.",
                UnpaidMembers = []
            };
        }

        var recipientIndex = _nextPayoutIndex;
        var recipient = _members[recipientIndex];
        var totalPot = WeeklyContribution * _members.Count;

        _payoutHistory.Add(new PayoutRecord(CurrentWeek, recipient, totalPot));
        _nextPayoutIndex = (_nextPayoutIndex + 1) % _members.Count;

        _paymentsForCurrentWeek.Clear();
        CurrentWeek++;

        return new WeekCloseResult
        {
            CanClose = true,
            Message = $"Week {CurrentWeek - 1} closed successfully. {recipient} received {MoneyFormatter.Format(totalPot)}.",
            Recipient = recipient,
            TotalPaid = totalPot
        };
    }

    public decimal GetPaidAmount(string memberName)
    {
        if (!_memberSet.Contains(memberName))
        {
            throw new InvalidOperationException($"Unknown member '{memberName}'.");
        }

        return _paymentsForCurrentWeek.TryGetValue(memberName, out var amount) ? amount : 0m;
    }

    public decimal GetOutstandingAmount(string memberName)
    {
        if (!_memberSet.Contains(memberName))
        {
            throw new InvalidOperationException($"Unknown member '{memberName}'.");
        }

        var totalPaid = GetPaidAmount(memberName);
        return Math.Max(0m, WeeklyContribution - totalPaid);
    }

    public IReadOnlyList<MemberPaymentStatus> GetMemberPaymentStatus()
    {
        return _members
            .Select(member => new MemberPaymentStatus
            {
                Name = member,
                AmountPaid = GetPaidAmount(member),
                AmountDue = GetOutstandingAmount(member),
                HasPaidInFull = GetPaidAmount(member) >= WeeklyContribution
            })
            .ToList();
    }

    public string GetReport()
    {
        var fullyPaid = GetMemberPaymentStatus().Where(x => x.HasPaidInFull).Select(x => x.Name).ToList();
        var owing = GetMemberPaymentStatus().Where(x => !x.HasPaidInFull).Select(x => $"{x.Name} owes {MoneyFormatter.Format(x.AmountDue)}").ToList();
        var payoutHistory = _payoutHistory.Count == 0
            ? "None"
            : string.Join("; ", _payoutHistory.Select(record => $"Week {record.Week}: {record.Recipient} received {MoneyFormatter.Format(record.Amount)}"));

        return $"Current week: {CurrentWeek}\n" +
               $"Fully paid: {(fullyPaid.Count == 0 ? "None" : string.Join(", ", fullyPaid))}\n" +
               $"Owing: {(owing.Count == 0 ? "None" : string.Join("; ", owing))}\n" +
               $"Payout history: {payoutHistory}";
    }

    private List<MemberDebt> GetUnpaidMembers()
    {
        return _members
            .Select(member => new MemberDebt
            {
                Name = member,
                AmountDue = GetOutstandingAmount(member)
            })
            .Where(member => member.AmountDue > 0m)
            .OrderBy(member => member.Name)
            .ToList();
    }
}

public sealed class MemberPaymentStatus
{
    public string Name { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public bool HasPaidInFull { get; set; }
}

public sealed class MemberDebt
{
    public string Name { get; set; } = string.Empty;
    public decimal AmountDue { get; set; }
}

public sealed class PayoutRecord
{
    public PayoutRecord(int week, string recipient, decimal amount)
    {
        Week = week;
        Recipient = recipient;
        Amount = amount;
    }

    public int Week { get; }
    public string Recipient { get; }
    public decimal Amount { get; }
}

public sealed class WeekCloseResult
{
    public bool CanClose { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Recipient { get; set; }
    public decimal TotalPaid { get; set; }
    public IReadOnlyList<MemberDebt> UnpaidMembers { get; set; } = Array.Empty<MemberDebt>();
}

public static class MoneyFormatter
{
    public static string Format(decimal value)
    {
        return $"GHS {value.ToString("0.00", CultureInfo.InvariantCulture)}";
    }
}
