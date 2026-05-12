using Habitee.Models;
using Microsoft.JSInterop;

namespace Habitee.Services;

public class DatabaseService
{
    private readonly IJSRuntime _jsRuntime;
    public event Action? DataChanged;

    public DatabaseService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.init");
    }

    public async Task<List<Habit>> GetAllHabitsAsync()
    {
        var habits = await _jsRuntime.InvokeAsync<List<Habit>>("habiteeDb.getAllHabits");
        return habits.Select(NormalizeHabit).ToList();
    }

    public async Task<List<Habit>> GetActiveHabitsAsync()
    {
        var habits = await GetAllHabitsAsync();
        return habits.Where(habit => !habit.IsArchived).ToList();
    }

    public async Task<List<Habit>> GetArchivedHabitsAsync()
    {
        var habits = await GetAllHabitsAsync();
        return habits.Where(habit => habit.IsArchived).ToList();
    }

    public async Task<Habit?> GetHabitByIdAsync(string id)
    {
        var habits = await GetAllHabitsAsync();
        return habits.FirstOrDefault(habit => habit.Id == id);
    }

    public async Task SaveHabitAsync(Habit habit)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.saveHabit", NormalizeHabit(habit));
        NotifyDataChanged();
    }

    public async Task DeleteHabitAsync(string id)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.deleteHabit", id);
        NotifyDataChanged();
    }

    public async Task ArchiveHabitAsync(string id)
    {
        var habit = await GetHabitByIdAsync(id);

        if (habit == null)
        {
            return;
        }

        habit.IsArchived = true;
        habit.ArchivedAt = DateTimeOffset.UtcNow.ToString("O");
        await SaveHabitAsync(habit);
    }

    public async Task RestoreHabitAsync(string id)
    {
        var habit = await GetHabitByIdAsync(id);

        if (habit == null)
        {
            return;
        }

        habit.IsArchived = false;
        habit.ArchivedAt = null;
        await SaveHabitAsync(habit);
    }

    public async Task<List<HabitLog>> GetLogsAsync()
    {
        return await _jsRuntime.InvokeAsync<List<HabitLog>>("habiteeDb.getLogs");
    }

    public async Task SaveLogAsync(HabitLog log)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.saveLog", log);
        NotifyDataChanged();
    }

    public async Task<string> ExportDataAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("habiteeDb.exportData");
    }

    public async Task<bool> ImportDataAsync(string jsonString)
    {
        var result = await _jsRuntime.InvokeAsync<bool>("habiteeDb.importData", jsonString);

        if (result)
        {
            NotifyDataChanged();
        }

        return result;
    }

    public async Task<bool> WasReminderSentAsync(string habitId, int reminderId, string date)
    {
        return await _jsRuntime.InvokeAsync<bool>("habiteeDb.wasReminderSent", habitId, reminderId, date);
    }

    public async Task MarkReminderSentAsync(SentReminder sentReminder)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.markReminderSent", sentReminder);
    }

    private void NotifyDataChanged()
    {
        DataChanged?.Invoke();
    }

    private static Habit NormalizeHabit(Habit habit)
    {
        habit.WeeklyDay = string.IsNullOrWhiteSpace(habit.WeeklyDay) ? "Mon" : habit.WeeklyDay;
        habit.MonthlyDay = Math.Clamp(habit.MonthlyDay, 1, 31);
        habit.WeeklyDaysCsv = string.IsNullOrWhiteSpace(habit.WeeklyDaysCsv) ? habit.WeeklyDay : habit.WeeklyDaysCsv;
        habit.MonthlyDaysCsv = string.IsNullOrWhiteSpace(habit.MonthlyDaysCsv) ? habit.MonthlyDay.ToString() : habit.MonthlyDaysCsv;
        habit.Reminders ??= new List<HabitReminder>();
        habit.Reminders = habit.Reminders
            .Where(reminder => reminder != null)
            .Take(Habit.MaxReminders)
            .Select(NormalizeReminder)
            .ToList();

        return habit;
    }

    private static HabitReminder NormalizeReminder(HabitReminder reminder)
    {
        reminder.TimeLocal = IsValidTime(reminder.TimeLocal) ? reminder.TimeLocal : "09:00";
        return reminder;
    }

    private static bool IsValidTime(string? value)
    {
        return TimeOnly.TryParse(value, out _);
    }
}
