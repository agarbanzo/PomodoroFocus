using PomodoroFocus.Domain.Enums;

namespace PomodoroFocus.Domain.Entities;

/// <summary>
/// Represents a Pomodoro session, managing the state of the timer, the type of session (work or break), the remaining time, and the count of completed Pomodoros. 
/// This class provides methods to start, pause, resume, and complete sessions, as well as to handle breaks and reset after long breaks. 
/// It serves as the core entity for tracking the user's progress through their Pomodoro workflow.
/// </summary>
public class PomodoroSession
{
    /// <summary>
    /// Gets the current state of the timer, which can be Ready, Running, Paused, Break, or Completed. 
    /// This property is used to track the lifecycle of the timer and control its behavior based on user interactions and session progress.
    /// </summary>
    public TimerState State { get; private set; }

    /// <summary>
    /// Gets the type of the current session, which can be a Pomodoro (focused work), ShortBreak, or LongBreak.
    /// </summary>
    /// <remarks>This property helps to differentiate between work sessions and break periods, 
    /// dallowing the application to manage transitions and apply appropriate logic based on the session type.</remarks>
    public SessionType CurrentSessionType { get; private set; }

    /// <summary>
    /// Gets the number of seconds remaining in the current session. 
    /// This property is updated as the timer counts down and is used to determine when a session has completed.
    /// </summary>
    public int RemainingSeconds { get; private set; }

    /// <summary>
    /// Gets the total number of completed Pomodoro sessions. 
    /// This count is incremented each time a Pomodoro session is completed and is used to track the user's progress through their work intervals, 
    /// as well as to determine when to take long breaks based on the configured threshold.
    /// </summary>
    public int CompletedPomodoros { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PomodoroSession"/> class with default values.
    /// </summary>
    public PomodoroSession()
    {
        State = TimerState.Ready;
        CurrentSessionType = SessionType.Pomodoro;
        RemainingSeconds = 0;
        CompletedPomodoros = 0;
    }

    /// <summary>
    /// Starts a new Pomodoro session with the specified duration in minutes.
    /// </summary>
    /// <param name="durationInMinutes">The duration in minutes.</param>
    public void Start(int durationInMinutes)
    {
        // Solo establecer como Pomodoro si no estamos en medio de un break
        if (State != TimerState.Break)
        {
            CurrentSessionType = SessionType.Pomodoro;
        }

        RemainingSeconds = durationInMinutes * 60;
        State = TimerState.Running;
    }

    /// <summary>
    /// Pauses the timer if it is currently running or in a break.
    /// </summary>
    public void Pause()
    {
        if (State == TimerState.Running || State == TimerState.Break)
            State = TimerState.Paused;
    }

    /// <summary>
    /// Resumes the timer if it is currently paused, restoring it to the appropriate state based on the current session type.
    /// </summary>
    public void Resume()
    {
        if (State == TimerState.Paused)
        {
            // If it was a Pomodoro, resume running;
            // If it was a break, resume the break state.
            State = CurrentSessionType == SessionType.Pomodoro
                ? TimerState.Running
                : TimerState.Break;
        }
    }

    /// <summary>
    /// Ticks the timer down by one second if it is currently running and there are remaining seconds.
    /// </summary>
    public void Tick()
    {
        if (State == TimerState.Running || State == TimerState.Break)
        {
            if (RemainingSeconds > 0)
            {
                RemainingSeconds--;
            }

            if (RemainingSeconds == 0)
            {
                State = TimerState.Completed;
            }
        }
    }

    /// <summary>
    /// Completes the current Pomodoro session, incrementing the count of completed Pomodoros if the current session is a Pomodoro, and resets the state to Ready.
    /// </summary>
    public void CompletePomodoro()
    {
        if (CurrentSessionType == SessionType.Pomodoro)
        {
            CompletedPomodoros++;
        }
        State = TimerState.Ready;
    }

    /// <summary>
    /// Cancels the current session, resetting the state to Ready and clearing the remaining seconds.
    /// </summary>
    public void Cancel()
    {
        State = TimerState.Ready;
        RemainingSeconds = 0;
    }

    /// <summary>
    /// Starts a break session of the specified type (ShortBreak or LongBreak) with the given duration and updates the state to Break.
    /// </summary>
    /// <param name="breakType">The type of break (ShortBreak or LongBreak)</param>
    /// <param name="minutes">The duration of the break in minutes</param>
    public void StartBreak(SessionType breakType, int minutes)
    {
        CurrentSessionType = breakType;
        State = TimerState.Break;
        RemainingSeconds = minutes * 60;
    }

    /// <summary>
    /// Resets the session after a long break, clearing the count of completed Pomodoros and setting the session type back to Pomodoro.
    /// </summary>
    public void ResetAfterLongBreak()
    {
        CompletedPomodoros = 0;
        CurrentSessionType = SessionType.Pomodoro;
    }
}
