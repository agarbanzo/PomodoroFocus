using PomodoroFocus.Domain.ValueObjects;

namespace PomodoroFocus.Application.Interfaces;

public interface ISessionConfigurationService
{
    TimeConfiguration CurrentConfiguration { get; }
    void UpdateConfiguration(TimeConfiguration newConfiguration);
    event Action? OnConfigurationChanged;
}