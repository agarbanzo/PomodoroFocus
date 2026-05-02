using PomodoroFocus.Application.Interfaces;
using PomodoroFocus.Domain.ValueObjects;

namespace PomodoroFocus.Application.Services;

public class SessionConfigurationService : ISessionConfigurationService
{
    private TimeConfiguration _config;

    public SessionConfigurationService(TimeConfiguration? config = null)
    {
        _config = config ?? new TimeConfiguration();
    }

    public TimeConfiguration CurrentConfiguration => _config;

    public event Action? OnConfigurationChanged;

    public void UpdateConfiguration(TimeConfiguration newConfiguration)
    {
        ArgumentNullException.ThrowIfNull(newConfiguration);
        _config = newConfiguration;
        OnConfigurationChanged?.Invoke();
    }
}