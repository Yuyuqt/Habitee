namespace Habitee.Models;

public class FirebaseMessageSummary
{
    public string Source { get; set; } = "foreground";
    public string Title { get; set; } = "Habitee";
    public string Body { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
}
