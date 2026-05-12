namespace Habitee.Models;

public class HabitReminder
{
    public int Id { get; set; }
    public string TimeLocal { get; set; } = "09:00";
    public bool Enabled { get; set; } = true;
}
