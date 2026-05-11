namespace Habitee.Models;

public class Habit
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "check"; // svg name or path
    public string Color { get; set; } = "var(--primary-color)";
    public int TargetCount { get; set; } = 1; // e.g. drink 8 glasses of water
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
