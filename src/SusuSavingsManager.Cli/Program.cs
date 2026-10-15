using System.Globalization;
using SusuSavingsManager.Core;

var app = new SusuApplication();
app.Run();

public sealed class SusuApplication
{
    private SusuGroup? _group;

    public void Run()
    {
        Console.WriteLine("Susu Group Tracker");
        Console.WriteLine("-------------------");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1. Create group");
            Console.WriteLine("2. Add member");
            Console.WriteLine("3. Record payment");
            Console.WriteLine("4. Close week");
            Console.WriteLine("5. Show report");
            Console.WriteLine("6. Exit");
            Console.Write("Choose an option: ");

            var option = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(option))
            {
                Console.WriteLine("Please choose a valid option.");
                continue;
            }

            try
            {
                switch (option.Trim())
                {
                    case "1":
                        CreateGroup();
                        break;
                    case "2":
                        AddMember();
                        break;
                    case "3":
                        RecordPayment();
                        break;
                    case "4":
                        CloseWeek();
                        break;
                    case "5":
                        ShowReport();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Unknown option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void CreateGroup()
    {
        Console.Write("Weekly contribution in GHS: ");
        var input = Console.ReadLine();
        var amount = ParseDecimal(input, "weekly contribution");

        _group = new SusuGroup(amount);
        Console.WriteLine($"Group created for {MoneyFormatter.Format(amount)} per week.");
    }

    private void AddMember()
    {
        EnsureGroupExists();
        Console.Write("Enter member name: ");
        var name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Member name cannot be empty.");
        }

        _group!.AddMember(name.Trim());
        Console.WriteLine($"Member '{name.Trim()}' added.");
    }

    private void RecordPayment()
    {
        EnsureGroupExists();
        Console.Write("Member name: ");
        var memberName = Console.ReadLine();
        Console.Write("Amount contributed in GHS: ");
        var amount = ParseDecimal(Console.ReadLine(), "payment amount");

        _group!.RecordPayment(memberName!.Trim(), amount);
        Console.WriteLine($"Payment recorded for {memberName.Trim()}. Remaining due: {MoneyFormatter.Format(_group.GetOutstandingAmount(memberName.Trim()))}");
    }

    private void CloseWeek()
    {
        EnsureGroupExists();
        var result = _group!.CloseCurrentWeek();
        Console.WriteLine(result.Message);

        if (!result.CanClose)
        {
            foreach (var member in result.UnpaidMembers)
            {
                Console.WriteLine($"- {member.Name} still owes {MoneyFormatter.Format(member.AmountDue)}");
            }
        }
    }

    private void ShowReport()
    {
        EnsureGroupExists();
        Console.WriteLine(_group!.GetReport());
    }

    private void EnsureGroupExists()
    {
        if (_group is null)
        {
            throw new InvalidOperationException("Create a group before using the susu tracker.");
        }
    }

    private static decimal ParseDecimal(string? input, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        if (!decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidOperationException($"{fieldName} must be a valid decimal amount.");
        }

        return value;
    }
}
