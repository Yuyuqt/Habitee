namespace Habitee.Models;

public class Habit
{
    public const int MaxReminders = 5;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "check"; // svg name or path
    public string Color { get; set; } = "var(--primary-color)";
    public int TargetCount { get; set; } = 1; // e.g. drink 8 glasses of water
    public string TrackingMode { get; set; } = "StepByStep";
    public string Frequency { get; set; } = "Daily";
    public string WeeklyDay { get; set; } = "Mon";
    public int MonthlyDay { get; set; } = 1;
    public string WeeklyDaysCsv { get; set; } = "Mon";
    public string MonthlyDaysCsv { get; set; } = "1";
    public List<HabitReminder> Reminders { get; set; } = new();
    public bool IsArchived { get; set; }
    public string? ArchivedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
