namespace Habitee.Models;

public class HabitLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserId { get; set; }
    public string HabitId { get; set; } = string.Empty;
    public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd"); // Use local date string
    public int CompletedCount { get; set; } = 0;
}
