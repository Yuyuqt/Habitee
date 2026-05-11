using Habitee.Models;
using Microsoft.JSInterop;

namespace Habitee.Services;

public class DatabaseService
{
    private readonly IJSRuntime _jsRuntime;

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
        return await _jsRuntime.InvokeAsync<List<Habit>>("habiteeDb.getAllHabits");
    }

    public async Task SaveHabitAsync(Habit habit)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.saveHabit", habit);
    }

    public async Task DeleteHabitAsync(string id)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.deleteHabit", id);
    }

    public async Task<List<HabitLog>> GetLogsAsync()
    {
        return await _jsRuntime.InvokeAsync<List<HabitLog>>("habiteeDb.getLogs");
    }

    public async Task SaveLogAsync(HabitLog log)
    {
        await _jsRuntime.InvokeVoidAsync("habiteeDb.saveLog", log);
    }

    public async Task<string> ExportDataAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("habiteeDb.exportData");
    }

    public async Task<bool> ImportDataAsync(string jsonString)
    {
        return await _jsRuntime.InvokeAsync<bool>("habiteeDb.importData", jsonString);
    }
}
