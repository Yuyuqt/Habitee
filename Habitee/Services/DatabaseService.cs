using Habitee.Models;
using Microsoft.JSInterop;

namespace Habitee.Services;

public class DatabaseService
{
    private readonly IJSRuntime _jsRuntime;
    private string? _currentUserId;

    public event Action? DataChanged;

    public string? CurrentUserId => _currentUserId;

    public DatabaseService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.init");
    }

    public void SetCurrentUser(string? userId)
    {
        _currentUserId = userId;
        NotifyDataChanged();
    }

    public async Task<List<Habit>> GetAllHabitsAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return new List<Habit>();
        }

        var habits = await _jsRuntime.InvokeAsync<List<Habit>>("habiteeDb.getAllHabits", _currentUserId);
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

    public async Task<List<Habit>> GetHabitsWithActiveRemindersAsync()
    {
        var habits = await GetAllHabitsAsync();
        return habits
            .Where(habit => !habit.IsArchived && habit.Reminders.Any(reminder => reminder.Enabled))
            .ToList();
    }

    public async Task<Habit?> GetHabitByIdAsync(string id)
    {
        var habits = await GetAllHabitsAsync();
        return habits.FirstOrDefault(habit => habit.Id == id);
    }

    public async Task SaveHabitAsync(Habit habit)
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return;
        }

        habit.UserId = _currentUserId;
        await _jsRuntime.InvokeVoidAsync("habiteeDb.saveHabit", NormalizeHabit(habit));
        NotifyDataChanged();
    }

    public async Task DeleteHabitAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return;
        }

        await _jsRuntime.InvokeVoidAsync("habiteeDb.deleteHabit", id, _currentUserId);
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
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return new List<HabitLog>();
        }

        return await _jsRuntime.InvokeAsync<List<HabitLog>>("habiteeDb.getLogs", _currentUserId);
    }

    public async Task SaveLogAsync(HabitLog log)
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return;
        }

        log.UserId = _currentUserId;
        await _jsRuntime.InvokeVoidAsync("habiteeDb.saveLog", log);
        NotifyDataChanged();
    }

    public async Task<HabitLog> CycleHabitCompletionAsync(Habit habit, DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        var dateKey = targetDate.ToString("yyyy-MM-dd");
        var existingLogs = await GetLogsAsync();
        var log = existingLogs.FirstOrDefault(entry => entry.HabitId == habit.Id && entry.Date == dateKey);

        if (log == null)
        {
            log = new HabitLog
            {
                HabitId = habit.Id,
                Date = dateKey,
                CompletedCount = 0
            };
        }

        log.CompletedCount = GetNextCompletedCount(log.CompletedCount, habit.TargetCount);
        await SaveLogAsync(log);
        return log;
    }

    public static int NormalizeCompletedCount(int completedCount, int targetCount)
    {
        var safeTargetCount = Math.Max(1, targetCount);
        var cycleSize = safeTargetCount + 1;
        var normalized = completedCount % cycleSize;
        return normalized < 0 ? normalized + cycleSize : normalized;
    }

    public static int GetNextCompletedCount(int completedCount, int targetCount)
    {
        var safeTargetCount = Math.Max(1, targetCount);
        var normalized = NormalizeCompletedCount(completedCount, safeTargetCount);
        return (normalized + 1) % (safeTargetCount + 1);
    }

    public static bool IsCompleteForTarget(int completedCount, int targetCount)
    {
        return NormalizeCompletedCount(completedCount, targetCount) == Math.Max(1, targetCount);
    }

    public async Task<string> ExportDataAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return "{\"habits\":[],\"logs\":[]}";
        }

        return await _jsRuntime.InvokeAsync<string>("habiteeDb.exportData", _currentUserId);
    }

    public async Task<bool> ImportDataAsync(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return false;
        }

        var result = await _jsRuntime.InvokeAsync<bool>("habiteeDb.importData", _currentUserId, jsonString);

        if (result)
        {
            NotifyDataChanged();
        }

        return result;
    }

    public async Task<bool> WasReminderSentAsync(string habitId, int reminderId, string date)
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return false;
        }

        return await _jsRuntime.InvokeAsync<bool>("habiteeDb.wasReminderSent", _currentUserId, habitId, reminderId, date);
    }

    public async Task MarkReminderSentAsync(SentReminder sentReminder)
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return;
        }

        sentReminder.UserId = _currentUserId;
        await _jsRuntime.InvokeVoidAsync("habiteeDb.markReminderSent", sentReminder);
    }

    public async Task<UserAccount?> GetUserByIdAsync(string userId)
    {
        return await _jsRuntime.InvokeAsync<UserAccount?>("habiteeDb.getUserById", userId);
    }

    public async Task<UserAccount?> GetUserByEmailAsync(string normalizedEmail)
    {
        return await _jsRuntime.InvokeAsync<UserAccount?>("habiteeDb.getUserByEmail", normalizedEmail);
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _jsRuntime.InvokeAsync<int>("habiteeDb.getUserCount");
    }

    public async Task SaveUserAsync(UserAccount user)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.saveUser", user);
    }

    public async Task ClaimLegacyDataAsync(string userId)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.claimLegacyData", userId);
        NotifyDataChanged();
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
