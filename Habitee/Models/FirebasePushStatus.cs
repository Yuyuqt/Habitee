namespace Habitee.Models;

public class FirebasePushStatus
{
    public bool IsSupported { get; set; }
    public string Permission { get; set; } = "default";
    public bool AppNotificationsEnabled { get; set; }
    public string FcmTokenStatus { get; set; } = "not-enabled";
    public string? FcmToken { get; set; }
    public string? LastError { get; set; }
    public FirebaseMessageSummary? LastMessageSummary { get; set; }
}
