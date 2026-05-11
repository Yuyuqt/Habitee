namespace Habitee.Services;

public class ConfettiService
{
    public event Action? OnBurst;

    public void TriggerBurst()
    {
        OnBurst?.Invoke();
    }
}
