namespace PomodoroFocus.Domain.Enums;

/// <summary>
/// Specifies the possible states of a timer during its lifecycle.
/// </summary>
/// <remarks>Use this enumeration to track and control the current status of a timer, such as whether it is ready
/// to start, actively running, paused, in a break period, or has completed its session. This can be useful for managing
/// timer behavior and responding to state changes in timer-based applications.</remarks>
public enum TimerState
{
    Ready,
    Running,
    Paused,
    Break,
    Completed
}
