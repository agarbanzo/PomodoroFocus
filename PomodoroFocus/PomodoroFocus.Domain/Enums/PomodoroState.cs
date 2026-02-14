namespace PomodoroFocus.Domain.Enums;

/// <summary>
/// Defines the various states of a Pomodoro timer, including work sessions, breaks, and paused states.
/// </summary>
public enum PomodoroState
{
    /// <summary>
    /// Initial state before starting a Pomodoro session, ready to begin work or take a break.
    /// </summary>
    Ready,

    /// <summary>
    /// Pomodoro work session in progress, where the user is actively focused on their task.
    /// </summary>
    Working,

    /// <summary>
    /// Pomodoro work session paused temporarily, allowing the user to take a break or attend to other matters before resuming work.
    /// </summary>
    WorkPaused,

    /// <summary>
    /// Short break in progress (typically 5-10 minutes), providing a brief rest period after a work session to recharge before the next Pomodoro session.
    /// </summary>
    ShortBreak,

    /// <summary>
    /// Long break in progress (typically 15-30 minutes), offering an extended rest period after completing a set of Pomodoro sessions, 
    /// allowing for deeper relaxation and recovery before starting the next cycle of work sessions.
    /// </summary>
    LongBreak,

    /// <summary>
    /// Break paused temporarily, allowing the user to attend to other matters before resuming their break or starting the next Pomodoro session.
    /// </summary>
    BreakPaused
}
