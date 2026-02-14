using PomodoroFocus.Domain.Enums;

namespace PomodoroFocus.Application.Interfaces;

/// <summary>
/// Represents a service for managing Pomodoro sessions, including starting, pausing, resuming, and canceling sessions, 
/// as well as tracking the current state of the timer, session type, remaining time, and completed Pomodoros.
/// </summary>
public interface IPomodoroService
{
    /// <summary>
    /// Gets the current state of the timer, which can be Ready, Running, Paused, Break, or Completed.
    /// </summary>
    TimerState CurrentState { get; }

    /// <summary>
    /// Gets the type of the current session, which can be a Pomodoro (focused work), ShortBreak, or LongBreak.
    /// </summary>
    SessionType CurrentSessionType { get; }

    /// <summary>
    /// Gets the number of seconds remaining in the current session. This value is updated as the timer counts down and is used to determine when a session has completed.
    /// </summary>
    int RemainingSeconds { get; }

    /// <summary>
    /// Gets the total number of completed Pomodoro sessions. 
    /// This count is incremented each time a Pomodoro session is completed and is used to track the user's progress through their work intervals, 
    /// as well as to determine when to take long breaks based on the configured threshold.
    /// </summary>
    int CompletedPomodoros { get; }

    /// <summary>
    /// Triggered on each timer tick (e.g., every second) to update the remaining time and check for session completion.
    /// </summary>
    event Action? OnTimerTick;

    /// <summary>
    /// Triggered when a session is completed, allowing subscribers to respond to the completion event, such as by updating the UI or starting a break.
    /// </summary>
    event Action? OnSessionComplete;

    /// <summary>
    /// Starts a new Pomodoro session with the configured duration for focused work. 
    /// This method initializes the timer and sets the appropriate state and session type for a work session.
    /// </summary>
    void StartPomodoro();

    /// <summary>
    /// Starts a break session based on the number of completed Pomodoros. 
    /// If the user has completed the configured number of Pomodoros before a long break, this method will start a long break; otherwise, 
    /// it will start a short break. This method also updates the timer state and session type accordingly.
    /// </summary>
    void StartBreak();

    /// <summary>
    /// Pauses the timer if it is currently running, allowing the user to temporarily stop the countdown without resetting the remaining time.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes the timer if it is currently paused, allowing the user to continue the countdown from where it was paused without resetting the remaining time.
    /// </summary>
    void Resume();

    /// <summary>
    /// Cancels the current session and resets the timer to its initial state. 
    /// This method can be used to stop an ongoing session and clear any progress, returning the timer to a ready state for starting a new session.
    /// </summary>
    void CancelAsCompleted();

    /// <summary>
    /// Cancels the current session without marking it as completed, 
    /// allowing the user to stop the timer and reset the remaining time without incrementing the count of completed Pomodoros.
    /// </summary>
    void CancelAsIncomplete();
}
