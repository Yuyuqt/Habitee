using Habitee.Models;
using Microsoft.JSInterop;

namespace Habitee.Services;

public sealed class AuthResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static AuthResult Ok() => new() { Success = true };
    public static AuthResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}

public sealed class AuthService
{
    private readonly DatabaseService _databaseService;
    private readonly IJSRuntime _jsRuntime;

    public AuthService(DatabaseService databaseService, IJSRuntime jsRuntime)
    {
        _databaseService = databaseService;
        _jsRuntime = jsRuntime;
    }

    public event Action? AuthStateChanged;

    public UserAccount? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;
    public bool IsInitialized { get; private set; }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        var sessionUserId = await _jsRuntime.InvokeAsync<string?>("habiteeAuth.getSessionUserId");

        if (!string.IsNullOrWhiteSpace(sessionUserId))
        {
            var user = await _databaseService.GetUserByIdAsync(sessionUserId);

            if (user != null)
            {
                ApplyAuthenticatedUser(user);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("habiteeAuth.clearSession");
                _databaseService.SetCurrentUser(null);
            }
        }
        else
        {
            _databaseService.SetCurrentUser(null);
        }

        IsInitialized = true;
        NotifyStateChanged();
    }

    public async Task<AuthResult> SignUpAsync(string displayName, string email, string password)
    {
        displayName = displayName.Trim();
        var normalizedEmail = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return AuthResult.Fail("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return AuthResult.Fail("Email is required.");
        }

        if (password.Length < 8)
        {
            return AuthResult.Fail("Password must be at least 8 characters.");
        }

        var existingUser = await _databaseService.GetUserByEmailAsync(normalizedEmail);

        if (existingUser != null)
        {
            return AuthResult.Fail("An account with that email already exists on this device.");
        }

        var existingUserCount = await _databaseService.GetUserCountAsync();
        var salt = await _jsRuntime.InvokeAsync<string>("habiteeAuth.generateSalt");
        var hash = await _jsRuntime.InvokeAsync<string>("habiteeAuth.hashPassword", password, salt);

        var user = new UserAccount
        {
            DisplayName = displayName,
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordSalt = salt,
            PasswordHash = hash
        };

        await _databaseService.SaveUserAsync(user);

        if (existingUserCount == 0)
        {
            await _databaseService.ClaimLegacyDataAsync(user.Id);
        }

        await PersistSessionAsync(user);
        return AuthResult.Ok();
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Fail("Enter your email and password.");
        }

        var user = await _databaseService.GetUserByEmailAsync(normalizedEmail);

        if (user == null)
        {
            return AuthResult.Fail("We couldn't find an account with that email on this device.");
        }

        var computedHash = await _jsRuntime.InvokeAsync<string>("habiteeAuth.hashPassword", password, user.PasswordSalt);

        if (!string.Equals(computedHash, user.PasswordHash, StringComparison.Ordinal))
        {
            return AuthResult.Fail("That password doesn't match this account.");
        }

        await PersistSessionAsync(user);
        return AuthResult.Ok();
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        _databaseService.SetCurrentUser(null);
        await _jsRuntime.InvokeVoidAsync("habiteeAuth.clearSession");
        NotifyStateChanged();
    }

    private async Task PersistSessionAsync(UserAccount user)
    {
        ApplyAuthenticatedUser(user);
        await _jsRuntime.InvokeVoidAsync("habiteeAuth.setSessionUserId", user.Id);
        NotifyStateChanged();
    }

    private void ApplyAuthenticatedUser(UserAccount user)
    {
        CurrentUser = user;
        _databaseService.SetCurrentUser(user.Id);
    }

    private void NotifyStateChanged()
    {
        AuthStateChanged?.Invoke();
    }

    private static string NormalizeEmail(string? email)
    {
        return email?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
