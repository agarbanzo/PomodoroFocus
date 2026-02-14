namespace PomodoroFocus.Domain.Enums;

/// <summary>
/// Specifies the type of session in a Pomodoro-based time management workflow.
/// </summary>
/// <remarks>Use this enumeration to distinguish between focused work sessions and breaks. The values correspond
/// to standard Pomodoro intervals: a Pomodoro session for focused work, a short break for brief rest periods, and a
/// long break for extended rest after several Pomodoro sessions.</remarks>
public enum SessionType
{
    Pomodoro,
    ShortBreak,
    LongBreak
}
