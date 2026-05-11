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
            MonthlyDaysCsv = "1"
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
    }
}
