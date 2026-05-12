namespace Habitee.Models;

public class SentReminder
{
    public string Id { get; set; } = string.Empty;
    public string HabitId { get; set; } = string.Empty;
    public int ReminderId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string SentAt { get; set; } = string.Empty;
}
