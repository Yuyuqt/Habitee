using Habitee.Models;

namespace Habitee.Services;

public class HabitDraftService
{
    public Habit Draft { get; private set; } = CreateDefaultDraft();
    public bool IsInitialized { get; private set; }

    public void Initialize(string? title, string? description, string? icon, string? color, bool reset)
    {
        if (!reset && IsInitialized)
        {
            return;
        }

        Draft = CreateDefaultDraft();

        if (!string.IsNullOrWhiteSpace(title))
        {
            Draft.Title = Uri.UnescapeDataString(title);
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            Draft.Description = Uri.UnescapeDataString(description);
        }

        if (!string.IsNullOrWhiteSpace(icon))
        {
            Draft.Icon = icon;
        }

        if (!string.IsNullOrWhiteSpace(color))
        {
            Draft.Color = Uri.UnescapeDataString(color);
        }

        NormalizeSelections(Draft);

        IsInitialized = true;
    }

    public void Reset()
    {
        Draft = CreateDefaultDraft();
        IsInitialized = false;
    }

    public void LoadFromHabit(Habit source)
    {
        Draft = CloneHabit(source);
        NormalizeSelections(Draft);
        IsInitialized = true;
    }

    private static Habit CreateDefaultDraft()
    {
        return new Habit
        {
            Color = "#10b981",
            TargetCount = 1,
            TrackingMode = "StepByStep",
            Frequency = "Daily",
            WeeklyDay = "Mon",
            MonthlyDay = 1,
            WeeklyDaysCsv = "Mon",
            MonthlyDaysCsv = "1",
            Reminders = new List<HabitReminder>()
        };
    }

    private static Habit CloneHabit(Habit source)
    {
        return new Habit
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            Icon = source.Icon,
            Color = source.Color,
            TargetCount = source.TargetCount,
            TrackingMode = source.TrackingMode,
            Frequency = source.Frequency,
            WeeklyDay = source.WeeklyDay,
            MonthlyDay = source.MonthlyDay,
            WeeklyDaysCsv = source.WeeklyDaysCsv,
            MonthlyDaysCsv = source.MonthlyDaysCsv,
            Reminders = source.Reminders
                .Select(reminder => new HabitReminder
                {
                    Id = reminder.Id,
                    TimeLocal = reminder.TimeLocal,
                    Enabled = reminder.Enabled
                })
                .ToList(),
            IsArchived = source.IsArchived,
            ArchivedAt = source.ArchivedAt,
            CreatedAt = source.CreatedAt
        };
    }

    private static void NormalizeSelections(Habit draft)
    {
        draft.MonthlyDay = Math.Clamp(draft.MonthlyDay, 1, 31);

        if (string.IsNullOrWhiteSpace(draft.WeeklyDaysCsv))
        {
            draft.WeeklyDaysCsv = string.IsNullOrWhiteSpace(draft.WeeklyDay) ? "Mon" : draft.WeeklyDay;
        }

        if (string.IsNullOrWhiteSpace(draft.MonthlyDaysCsv))
        {
            draft.MonthlyDaysCsv = draft.MonthlyDay.ToString();
        }

        draft.WeeklyDay = draft.WeeklyDaysCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "Mon";
        draft.MonthlyDay = draft.MonthlyDaysCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var parsed) ? parsed : 1)
            .Where(value => value >= 1 && value <= 31)
            .DefaultIfEmpty(1)
            .Min();

        draft.Reminders ??= new List<HabitReminder>();
        draft.Reminders = draft.Reminders
            .Take(Habit.MaxReminders)
            .Select(reminder =>
            {
                reminder.TimeLocal = TimeOnly.TryParse(reminder.TimeLocal, out _) ? reminder.TimeLocal : "09:00";
                return reminder;
            })
            .ToList();
    }
}
