using Microsoft.JSInterop;

namespace Habitee.Services;

public class ThemeState
{
    public string Theme { get; set; } = "habitee";
    public string Mode { get; set; } = "light";
}

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    
    public string CurrentTheme { get; private set; } = "habitee";
    public string CurrentMode { get; private set; } = "light";

    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        var state = await _jsRuntime.InvokeAsync<ThemeState>("themeInterop.init");
        CurrentTheme = state.Theme;
        CurrentMode = state.Mode;
        NotifyStateChanged();
    }

    public async Task SetThemeAsync(string theme)
    {
        CurrentTheme = theme;
        await ApplyThemeAsync();
    }

    public async Task SetModeAsync(string mode)
    {
        CurrentMode = mode;
        await ApplyThemeAsync();
    }
    
    public async Task ToggleModeAsync()
    {
        CurrentMode = CurrentMode == "light" ? "dark" : "light";
        await ApplyThemeAsync();
    }

    private async Task ApplyThemeAsync()
    {
        await _jsRuntime.InvokeVoidAsync("themeInterop.setTheme", CurrentTheme, CurrentMode);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnThemeChanged?.Invoke();
}
