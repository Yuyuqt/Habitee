using Habitee.Models;
using Microsoft.JSInterop;

namespace Habitee.Services;

public sealed class NotificationService : IAsyncDisposable
{
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan HeartbeatLookback = TimeSpan.FromSeconds(70);
    private static readonly TimeSpan ResumeCatchupWindow = TimeSpan.FromMinutes(5);

    private readonly IJSRuntime _jsRuntime;
    private readonly DatabaseService _databaseService;
    private readonly SemaphoreSlim _evaluationLock = new(1, 1);
    private CancellationTokenSource? _heartbeatCts;
    private Task? _heartbeatTask;
    private DotNetObjectReference<NotificationService>? _dotNetReference;
    private bool _initialized;
    private bool _visibilityCallbackRegistered;
    private bool _isVisible = true;

    public NotificationService(IJSRuntime jsRuntime, DatabaseService databaseService)
    {
        _jsRuntime = jsRuntime;
        _databaseService = databaseService;
    }

    public event Action? StatusChanged;

    public bool IsSupported { get; private set; }
    public string Permission { get; private set; } = "default";
    public bool AppNotificationsEnabled { get; private set; }
    public bool CanNotify => IsSupported && Permission == "granted" && AppNotificationsEnabled;
    public bool IsFirebasePushSupported { get; private set; }
    public string FcmTokenStatus { get; private set; } = "not-enabled";
    public string? FcmToken { get; private set; }
    public string? FcmLastError { get; private set; }
    public FirebaseMessageSummary? LastMessageSummary { get; private set; }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            await RefreshStatusCoreAsync();
            _dotNetReference = DotNetObjectReference.Create(this);
            _isVisible = await _jsRuntime.InvokeAsync<bool>("habiteeNotifications.registerVisibilityCallback", _dotNetReference);
            _visibilityCallbackRegistered = true;
            _databaseService.DataChanged += HandleDataChanged;
            _initialized = true;

            UpdateHeartbeatState();
        }
        catch (Exception)
        {
            IsSupported = false;
            Permission = "unsupported";
            StopHeartbeat();
        }

        await RefreshFirebasePushStateAsync();
        NotifyStatusChanged();
    }

    public async Task RequestPushPermissionAsync()
    {
        try
        {
            var status = await _jsRuntime.InvokeAsync<FirebasePushStatus>("habiteeFirebaseMessaging.requestPermissionAndToken");
            ApplyFirebaseStatus(status);
        }
        catch (Exception ex)
        {
            IsFirebasePushSupported = false;
            FcmTokenStatus = "error";
            FcmToken = null;
            FcmLastError = ex.Message;
        }

        await RefreshStatusAsync();
    }

    public async Task EnableNotificationsAsync()
    {
        await RequestPushPermissionAsync();

        if (CanNotify)
        {
            await RunDueReminderCheckSafeAsync(HeartbeatLookback);
        }
    }

    public async Task DisableNotificationsAsync()
    {
        try
        {
            var status = await _jsRuntime.InvokeAsync<FirebasePushStatus>("habiteeFirebaseMessaging.disablePush");
            ApplyFirebaseStatus(status);
        }
        catch (Exception ex)
        {
            FcmLastError = ex.Message;
        }

        StopHeartbeat();
        NotifyStatusChanged();
    }

    public async Task RefreshFcmTokenAsync()
    {
        try
        {
            var status = await _jsRuntime.InvokeAsync<FirebasePushStatus>("habiteeFirebaseMessaging.refreshToken");
            ApplyFirebaseStatus(status);
        }
        catch (Exception ex)
        {
            IsFirebasePushSupported = false;
            FcmTokenStatus = "error";
            FcmToken = null;
            FcmLastError = ex.Message;
        }

        await RefreshStatusAsync();
    }

    public async Task<bool> CopyFcmTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("habiteeFirebaseMessaging.copyToken");
        }
        catch
        {
            return false;
        }
    }

    public async Task RefreshStatusAsync()
    {
        try
        {
            await RefreshStatusCoreAsync();
            UpdateHeartbeatState();
            await RefreshFirebasePushStateAsync();
        }
        catch (Exception)
        {
            IsSupported = false;
            Permission = "unsupported";
            StopHeartbeat();
        }

        NotifyStatusChanged();
    }

    public async Task<string> RequestPermissionAsync()
    {
        await EnableNotificationsAsync();
        return Permission;
    }

    [JSInvokable]
    public Task HandleVisibilityChanged(bool isVisible)
    {
        _isVisible = isVisible;

        if (isVisible)
        {
            _ = HandleResumeSafeAsync();
        }

        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task HandleFirebaseMessage(FirebaseMessageSummary messageSummary)
    {
        LastMessageSummary = messageSummary;
        NotifyStatusChanged();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _databaseService.DataChanged -= HandleDataChanged;
        StopHeartbeat();

        if (_visibilityCallbackRegistered)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("habiteeNotifications.unregisterVisibilityCallback");
            }
            catch
            {
                // Ignore disposal-time JS failures.
            }
        }

        _dotNetReference?.Dispose();
        _evaluationLock.Dispose();
    }

    private void HandleDataChanged()
    {
        if (CanNotify && _isVisible)
        {
            _ = RunDueReminderCheckSafeAsync(HeartbeatLookback);
        }
    }

    private async Task HandleResumeSafeAsync()
    {
        try
        {
            await HandleResumeAsync();
        }
        catch (Exception)
        {
            IsSupported = false;
            Permission = "unsupported";
            StopHeartbeat();
            NotifyStatusChanged();
        }
    }

    private async Task HandleResumeAsync()
    {
        await RefreshStatusCoreAsync();
        UpdateHeartbeatState();
        await RefreshFirebasePushStateAsync();
        NotifyStatusChanged();

        if (CanNotify)
        {
            await EvaluateDueRemindersAsync(ResumeCatchupWindow);
        }
    }

    private async Task RefreshStatusCoreAsync()
    {
        IsSupported = await _jsRuntime.InvokeAsync<bool>("habiteeNotifications.isSupported");
        Permission = IsSupported
            ? await _jsRuntime.InvokeAsync<string>("habiteeNotifications.getPermission")
            : "unsupported";
    }

    private async Task RefreshFirebasePushStateAsync()
    {
        try
        {
            if (_dotNetReference == null)
            {
                _dotNetReference = DotNetObjectReference.Create(this);
            }

            var status = await _jsRuntime.InvokeAsync<FirebasePushStatus>("habiteeFirebaseMessaging.initialize", _dotNetReference);
            ApplyFirebaseStatus(status);
        }
        catch (Exception ex)
        {
            IsFirebasePushSupported = false;
            FcmTokenStatus = "error";
            FcmToken = null;
            FcmLastError = ex.Message;
        }
    }

    private void UpdateHeartbeatState()
    {
        if (CanNotify)
        {
            EnsureHeartbeat();
        }
        else
        {
            StopHeartbeat();
        }
    }

    private void EnsureHeartbeat()
    {
        if (_heartbeatTask is { IsCompleted: false })
        {
            return;
        }

        _heartbeatCts = new CancellationTokenSource();
        _heartbeatTask = RunHeartbeatAsync(_heartbeatCts.Token);
    }

    private void StopHeartbeat()
    {
        if (_heartbeatCts == null)
        {
            return;
        }

        _heartbeatCts.Cancel();
        _heartbeatCts.Dispose();
        _heartbeatCts = null;
        _heartbeatTask = null;
    }

    private async Task RunHeartbeatAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextTick = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind)
                    .Add(HeartbeatInterval)
                    .AddSeconds(1);
                var delay = nextTick - now;

                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.Zero;
                }

                await Task.Delay(delay, cancellationToken);

                try
                {
                    await EvaluateDueRemindersAsync(HeartbeatLookback, cancellationToken);
                }
                catch (Exception)
                {
                    IsSupported = false;
                    Permission = "unsupported";
                    StopHeartbeat();
                    NotifyStatusChanged();
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown or permission changes.
        }
    }

    private async Task RunDueReminderCheckSafeAsync(TimeSpan lookback)
    {
        try
        {
            await EvaluateDueRemindersAsync(lookback);
        }
        catch (Exception)
        {
            IsSupported = false;
            Permission = "unsupported";
            StopHeartbeat();
            NotifyStatusChanged();
        }
    }

    private async Task EvaluateDueRemindersAsync(TimeSpan lookback, CancellationToken cancellationToken = default)
    {
        if (!CanNotify)
        {
            return;
        }

        await _evaluationLock.WaitAsync(cancellationToken);

        try
        {
            var now = DateTime.Now;
            var windowStart = now - lookback;

            if (windowStart.Date != now.Date)
            {
                windowStart = now.Date;
            }

            var todayKey = now.ToString("yyyy-MM-dd");
            var habits = await _databaseService.GetActiveHabitsAsync();
            var logs = await _databaseService.GetLogsAsync();
            var todayLogs = logs
                .Where(log => log.Date == todayKey)
                .GroupBy(log => log.HabitId)
                .ToDictionary(group => group.Key, group => group.First());

            foreach (var habit in habits)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (habit.Reminders.Count == 0 || !IsHabitDueToday(habit, now.Date))
                {
                    continue;
                }

                if (IsHabitCompleteForToday(habit, todayLogs))
                {
                    continue;
                }

                foreach (var reminder in habit.Reminders.Where(reminder => reminder.Enabled))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!TimeOnly.TryParse(reminder.TimeLocal, out var reminderTime))
                    {
                        continue;
                    }

                    var scheduledAt = now.Date.Add(reminderTime.ToTimeSpan());

                    if (scheduledAt < windowStart || scheduledAt > now)
                    {
                        continue;
                    }

                    var reminderDate = scheduledAt.ToString("yyyy-MM-dd");
                    var alreadySent = await _databaseService.WasReminderSentAsync(habit.Id, reminder.Id, reminderDate);

                    if (alreadySent || IsHabitCompleteForToday(habit, todayLogs))
                    {
                        continue;
                    }

                    var shown = await _jsRuntime.InvokeAsync<bool>(
                        "habiteeNotifications.showNotification",
                        habit.Title,
                        new
                        {
                            body = "Time to work on this habit.",
                            icon = "/icon-192.png",
                            tag = $"habit-reminder-{habit.Id}-{reminder.Id}-{reminderDate}",
                            url = "/"
                        });

                    if (!shown)
                    {
                        continue;
                    }

                    await _databaseService.MarkReminderSentAsync(new SentReminder
                    {
                        Id = BuildSentReminderId(habit.Id, reminder.Id, reminderDate),
                        HabitId = habit.Id,
                        ReminderId = reminder.Id,
                        Date = reminderDate,
                        SentAt = DateTimeOffset.Now.ToString("O")
                    });
                }
            }
        }
        finally
        {
            _evaluationLock.Release();
        }
    }

    private static bool IsHabitCompleteForToday(Habit habit, IReadOnlyDictionary<string, HabitLog> todayLogs)
    {
        return todayLogs.TryGetValue(habit.Id, out var log) && log.CompletedCount >= habit.TargetCount;
    }

    private static bool IsHabitDueToday(Habit habit, DateTime today)
    {
        return habit.Frequency switch
        {
            "Weekly" => GetWeeklySelections(habit).Contains(ToWeeklyLabel(today.DayOfWeek)),
            "Monthly" => GetMonthlySelections(habit).Contains(today.Day),
            _ => true
        };
    }

    private static HashSet<string> GetWeeklySelections(Habit habit)
    {
        var selectedDays = habit.WeeklyDaysCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(day => !string.IsNullOrWhiteSpace(day))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (selectedDays.Count == 0)
        {
            selectedDays.Add(string.IsNullOrWhiteSpace(habit.WeeklyDay) ? "Mon" : habit.WeeklyDay);
        }

        return selectedDays;
    }

    private static HashSet<int> GetMonthlySelections(Habit habit)
    {
        var selectedDays = habit.MonthlyDaysCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var parsed) ? parsed : -1)
            .Where(value => value >= 1 && value <= 31)
            .ToHashSet();

        if (selectedDays.Count == 0)
        {
            selectedDays.Add(Math.Clamp(habit.MonthlyDay, 1, 31));
        }

        return selectedDays;
    }

    private static string ToWeeklyLabel(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Mon",
            DayOfWeek.Tuesday => "Tue",
            DayOfWeek.Wednesday => "Wed",
            DayOfWeek.Thursday => "Thu",
            DayOfWeek.Friday => "Fri",
            DayOfWeek.Saturday => "Sat",
            _ => "Sun"
        };
    }

    private static string BuildSentReminderId(string habitId, int reminderId, string date)
    {
        return $"{habitId}|{reminderId}|{date}";
    }

    private void NotifyStatusChanged()
    {
        StatusChanged?.Invoke();
    }

    private void ApplyFirebaseStatus(FirebasePushStatus? status)
    {
        if (status == null)
        {
            IsFirebasePushSupported = false;
            FcmTokenStatus = "error";
            FcmToken = null;
            FcmLastError = "Firebase Messaging did not return a status payload.";
            return;
        }

        IsFirebasePushSupported = status.IsSupported;
        AppNotificationsEnabled = status.AppNotificationsEnabled;
        FcmTokenStatus = status.FcmTokenStatus;
        FcmToken = string.IsNullOrWhiteSpace(status.FcmToken) ? null : status.FcmToken;
        FcmLastError = string.IsNullOrWhiteSpace(status.LastError) ? null : status.LastError;

        if (status.LastMessageSummary != null)
        {
            LastMessageSummary = status.LastMessageSummary;
        }
    }
}
