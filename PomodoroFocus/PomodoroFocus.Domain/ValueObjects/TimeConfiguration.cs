namespace PomodoroFocus.Domain.ValueObjects;

/// <summary>
/// Represents the configuration settings for a Pomodoro timer, including durations for work sessions and breaks.
/// </summary>
/// <remarks>This record allows users to customize the lengths of Pomodoro sessions, short breaks, and long
/// breaks, as well as the number of Pomodoros before a long break. The default values are set to 25 minutes for
/// Pomodoro duration, 5 minutes for short breaks, 15 minutes for long breaks, and 4 Pomodoros before a long
/// break.</remarks>
public record TimeConfiguration
{
    /// <summary>
    /// Gets the duration of a Pomodoro session in minutes. The default value is 25 minutes, which is the standard length for a Pomodoro session. Users can customize this value to fit their preferred work intervals.
    /// </summary>
    public int PomodoroDuration { get; init; } = 25; // minutes.

    /// <summary>
    /// Gets the duration of a short break in minutes. The default value is 5 minutes, which is a common length for a short break between Pomodoro sessions. Users can adjust this value to suit their rest preferences.
    /// </summary>
    public int ShortBreakDuration { get; init; } = 5;

    /// <summary>
    /// Gets the duration of a long break in minutes. The default value is 15 minutes, which is a typical length for a long break after several Pomodoro sessions. Users can modify this value to accommodate their desired rest periods after completing multiple Pomodoros.
    /// </summary>
    public int LongBreakDuration { get; init; } = 15;

    /// <summary>
    /// Gets the number of Pomodoro sessions that must be completed before taking a long break. The default value is 4, which means that after completing four Pomodoro sessions, the user will take a long break. Users can change this value to set their own threshold for when to take a long break based on their work habits and preferences.
    /// </summary>
    public int PomodorosBeforeLongBreak { get; init; } = 4;
}