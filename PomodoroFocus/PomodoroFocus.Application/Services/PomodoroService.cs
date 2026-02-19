using PomodoroFocus.Application.Interfaces;
using PomodoroFocus.Domain.Entities;
using PomodoroFocus.Domain.Enums;
using PomodoroFocus.Domain.ValueObjects;
using System.Timers;

namespace PomodoroFocus.Application.Services;

/// <summary>
/// Implements the <see cref="IPomodoroService"/> interface to manage Pomodoro sessions, including starting, pausing, resuming, and canceling sessions,
/// </summary>
public class PomodoroService : IPomodoroService, IDisposable
{
    private readonly PomodoroSession _session;
    private readonly TimeConfiguration _config;
    private readonly System.Timers.Timer _timer;

    /// <summary>
    /// Gets the current state of the timer, which can be Ready, Running, Paused, Break, or Completed. 
    /// This property is used to track the lifecycle of the timer and control its behavior based on user interactions and session progress.
    /// </summary>
    public TimerState CurrentState => _session.State;

    /// <summary>
    /// Gets the type of the current session, which can be a Pomodoro (focused work), ShortBreak, or LongBreak.
    /// </summary>
    public SessionType CurrentSessionType => _session.CurrentSessionType;

    /// <summary>
    /// Gets the number of seconds remaining in the current session.
    /// </summary>
    public int RemainingSeconds => _session.RemainingSeconds;

    /// <summary>
    /// Completed Pomodoros count is incremented each time a Pomodoro session is completed and is used to track the user's progress through their work intervals.
    /// </summary>
    public int CompletedPomodoros => _session.CompletedPomodoros;

    /// <summary>
    /// Triggered on each timer tick (e.g., every second) to update the remaining time and check for session completion.
    /// </summary>
    public event Action? OnTimerTick;

    /// <summary>
    /// Triggered when a session is completed, allowing subscribers to respond to the completion event, such as by updating the UI or starting a break.
    /// </summary>
    public event Action? OnSessionComplete;

    /// <summary>
    /// Initializes a new instance of the <see cref="PomodoroService"/> class with an optional <see cref="TimeConfiguration"/> parameter.
    /// </summary>
    /// <param name="config"></param>
    public PomodoroService(TimeConfiguration? config = null)
    {
        _session = new PomodoroSession();
        _config = config ?? new TimeConfiguration();
        _timer = new System.Timers.Timer(1000); // 1 second interval.
        _timer.Elapsed += TimerElapsed;
    }

    /// <summary>
    /// Starts a new Pomodoro session with the configured duration for focused work.
    /// </summary>
    public void StartPomodoro()
    {
        _session.Start(_config.PomodoroDuration);
        _timer.Start();
    }

    /// <summary>
    /// Starts a break session, determining whether it should be a short break or a long break 
    /// based on the number of completed Pomodoros and the configured thresholds.
    /// If starting a long break, resets the Pomodoro counter.
    /// </summary>
    public void StartBreak()
    {
        // Determine break type: Long break after every N Pomodoros, otherwise short
        var breakType = _session.CompletedPomodoros > 0
            && _session.CompletedPomodoros % _config.PomodorosBeforeLongBreak == 0
            ? SessionType.LongBreak
            : SessionType.ShortBreak;

        var duration = breakType == SessionType.LongBreak
            ? _config.LongBreakDuration
            : _config.ShortBreakDuration;

        _session.StartBreak(breakType, duration);
        _timer.Start();

        // Reset counter after starting long break
        if (breakType == SessionType.LongBreak)
        {
            _session.ResetAfterLongBreak();
        }
    }

    /// <summary>
    /// Pauses the timer if it is currently running, allowing the user to temporarily stop the countdown without resetting the remaining time.
    /// </summary>
    public void Pause()
    {
        _session.Pause();
        _timer.Stop();
    }

    /// <summary>
    /// Pauses the timer if it is currently paused, allowing the user to resume the countdown from where it left off without resetting the remaining time.
    /// </summary>
    public void Resume()
    {
        _session.Resume();
        _timer.Start();
    }

    /// <summary>
    /// Cancels the current session and marks it as completed, stopping the timer and updating the session state accordingly.
    /// </summary>
    public void CancelAsCompleted()
    {
        _timer.Stop();

        // Solo incrementar contador si es un Pomodoro
        if (_session.CurrentSessionType == SessionType.Pomodoro)
        {
            _session.CompletePomodoro();
            OnSessionComplete?.Invoke(); // Notify UI
        }
        else
        {
            // Si es un break, solo cambiar a Ready sin incrementar
            _session.CompleteBreak();
            // Should we fire OnSessionComplete for break? 
            // Usually breaks just "end". But if we want to play sound or update UI state, maybe yes.
            // But UC says "Mark as Completed" usually applies to Work. 
            // If I cancel a break "as completed"... effectively I skipped it?
            // Let's fire it to be safe for UI updates.
            OnSessionComplete?.Invoke();
        }
    }

    /// <summary>
    /// Cancels the current session without marking it as completed, stopping the timer and resetting the session.
    /// </summary>
    public void CancelAsIncomplete()
    {
        _timer.Stop();

        if (_session.CurrentSessionType == SessionType.Pomodoro)
        {
            _session.Cancel(); // Reset Pomodoro
        }
        else
        {
            _session.CompleteBreak(); // Reset Break to Ready
        }
    }

    /// <summary>
    /// Timer event handler that is called on each tick of the timer (e.g., every second) to update the session's remaining time, 
    /// trigger the OnTimerTick event, and check for session completion to trigger the OnSessionComplete event if necessary.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _session.Tick();
        OnTimerTick?.Invoke();

        if (_session.State == TimerState.Completed)
        {
            _timer.Stop();
            OnSessionComplete?.Invoke();
        }
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="PomodoroService"/> class, including stopping and disposing of the timer to free up system resources.
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose();
    }
}
