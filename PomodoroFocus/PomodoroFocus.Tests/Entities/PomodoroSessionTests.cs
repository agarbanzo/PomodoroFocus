using PomodoroFocus.Domain.Entities;
using PomodoroFocus.Domain.Enums;

namespace PomodoroFocus.Tests.Entities;

/// <summary>
/// Implements unit tests for the <see cref="PomodoroSession"/> class, covering all core functionalities such as starting sessions, pausing, resuming, completing Pomodoros, 
/// handling breaks, and resetting after long breaks.
/// </summary>
[TestFixture]
public class PomodoroSessionTests
{
    private PomodoroSession _session = null!;

    /// <summary>
    /// Sets up a new instance of <see cref="PomodoroSession"/> before each test to ensure a clean state for testing. 
    /// This method is executed before every test case, allowing us to verify the behavior of the session under consistent initial conditions.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _session = new PomodoroSession();
    }

    #region Initialization Tests

    /// <summary>
    /// Tests that the constructor of <see cref="PomodoroSession"/> initializes the session with the correct default values.
    /// </summary>
    [Test]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.Pomodoro));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(0));
            Assert.That(_session.CompletedPomodoros, Is.EqualTo(0));
        });
    }

    #endregion

    #region UC-01: Start Tests

    /// <summary>
    /// Tests that starting a Pomodoro session sets the correct duration in seconds and changes the state to Running.
    /// </summary>
    [Test]
    public void Start_ShouldSetCorrectDurationAndState()
    {
        // Arrange
        const int durationInMinutes = 25;

        // Act
        _session.Start(durationInMinutes);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Running));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(durationInMinutes * 60));
        });
    }

    /// <summary>
    /// Tests that starting a Pomodoro session with different durations correctly sets the remaining seconds based on the input duration in minutes.
    /// </summary>
    /// <param name="durationInMinutes"></param>
    [Test]
    public void Start_WithDifferentDurations_ShouldSetCorrectSeconds(
        [Values(1, 5, 25, 60)] int durationInMinutes)
    {
        // Act
        _session.Start(durationInMinutes);

        // Assert
        Assert.That(_session.RemainingSeconds, Is.EqualTo(durationInMinutes * 60));
    }

    #endregion

    #region UC-02: Pause and Resume Tests (Pomodoro)

    /// <summary>
    /// Tests that pausing a running Pomodoro session changes the state to Paused, allowing the user to temporarily halt the timer without losing progress.
    /// </summary>
    [Test]
    public void Pause_FromRunning_ShouldChangeToPaused()
    {
        // Arrange
        _session.Start(25);

        // Act
        _session.Pause();

        // Assert
        Assert.That(_session.State, Is.EqualTo(TimerState.Paused));
    }

    /// <summary>
    /// Tests that pausing a session that is in the Ready state does not change the state, 
    /// ensuring that the pause functionality only affects active sessions and does not cause unintended side effects when the timer is not running.
    /// </summary>
    [Test]
    public void Pause_FromReady_ShouldNotChangeState()
    {
        // Act
        _session.Pause();

        // Assert
        Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
    }

    /// <summary>
    /// Tests that resuming a paused Pomodoro session changes the state back to Running and retains the correct session type,
    /// </summary>
    [Test]
    public void Resume_FromPausedPomodoro_ShouldReturnToRunning()
    {
        // Arrange
        _session.Start(25);
        _session.Pause();

        // Act
        _session.Resume();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Running));
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.Pomodoro));
        });
    }

    /// <summary>
    /// Tests that resuming a session that is in the Ready state does not change the state,
    /// </summary>
    [Test]
    public void Resume_FromReady_ShouldNotChangeState()
    {
        // Act
        _session.Resume();

        // Assert
        Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
    }

    /// <summary>
    /// Tests that pausing and then resuming a Pomodoro session preserves the remaining time, 
    /// ensuring that users can take breaks without losing their progress in the current session.
    /// </summary>
    [Test]
    public void PauseAndResume_ShouldPreserveRemainingTime()
    {
        // Arrange
        _session.Start(25);
        var initialTime = _session.RemainingSeconds;

        // Act
        _session.Pause();
        var pausedTime = _session.RemainingSeconds;
        _session.Resume();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(pausedTime, Is.EqualTo(initialTime));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(initialTime));
        });
    }

    #endregion

    #region UC-06: Pause, Resume and Cancel Tests (Break)
    /// <summary>
    /// Tests that pausing a running break session changes the state to Paused, allowing users to temporarily halt their break without losing progress, 
    /// similar to how pausing works during a Pomodoro session.
    /// </summary>
    [Test]
    public void Pause_FromBreak_ShouldChangeToPaused(
        [Values(5, 15)] int durationInMinutes)
    {
        // Arrange
        _session.StartBreak(SessionType.ShortBreak, durationInMinutes);
        _session.Start(5);

        // Act
        _session.Pause();

        // Assert
        Assert.That(_session.State, Is.EqualTo(TimerState.Paused));
    }

    /// <summary>
    /// Tests that resuming a paused break session changes the state back to Break and retains the correct session type,
    /// </summary>
    [Test]
    public void Resume_FromPausedShortBreak_ShouldReturnToBreak()
    {
        // Arrange
        _session.StartBreak(SessionType.ShortBreak, 5);
        _session.Start(5);
        _session.Pause();

        // Act
        _session.Resume();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Break));
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak));
        });
    }

    /// <summary>
    /// Tests that resuming a paused long break session changes the state back to Break and retains the correct session type.
    /// </summary>
    [Test]
    public void Resume_FromPausedLongBreak_ShouldReturnToBreak()
    {
        // Arrange
        _session.StartBreak(SessionType.LongBreak, 15);
        _session.Start(15);
        _session.Pause();

        // Act
        _session.Resume();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Break));
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.LongBreak));
        });
    }

    /// <summary>
    /// Tests that canceling a break session, whether it's a short or long break, returns the session to the Ready state and resets the remaining time to zero.
    /// </summary>
    [Test]
    public void Cancel_FromBreak_ShouldReturnToReadyAndResetTime()
    {
        // Arrange
        _session.StartBreak(SessionType.LongBreak, 15);
        _session.Start(15);

        // Act
        _session.Cancel();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Tests that canceling a running Pomodoro session returns the session to the Ready state and resets the remaining time to zero.
    /// </summary>
    [Test]
    public void Cancel_FromRunning_ShouldReturnToReadyAndResetTime()
    {
        // Arrange
        _session.Start(25);

        // Act
        _session.Cancel();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Tests that canceling a paused Pomodoro session returns the session to the Ready state and resets the remaining time to zero.
    /// </summary>
    [Test]
    public void Cancel_FromPaused_ShouldReturnToReadyAndResetTime()
    {
        // Arrange
        _session.Start(25);
        _session.Pause();

        // Act
        _session.Cancel();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(0));
        });
    }

    #endregion

    #region Tick Tests
    /// <summary>
    /// Tests that calling the Tick method while the session is running decrements the remaining seconds by one, simulating the passage of time during an active session.
    /// </summary>
    [Test]
    public void Tick_WhenRunning_ShouldDecrementSeconds()
    {
        // Arrange
        _session.Start(1);
        var initialSeconds = _session.RemainingSeconds;

        // Act
        _session.Tick();

        // Assert
        Assert.That(_session.RemainingSeconds, Is.EqualTo(initialSeconds - 1));
    }
    /// <summary>
    /// Tests that calling the Tick method while the session is paused does not change the remaining seconds, ensuring that time does not progress when the timer is paused.
    /// </summary>
    [Test]
    public void Tick_WhenPaused_ShouldNotDecrementSeconds()
    {
        // Arrange
        _session.Start(25);
        _session.Pause();
        var pausedSeconds = _session.RemainingSeconds;

        // Act
        _session.Tick();

        // Assert
        Assert.That(_session.RemainingSeconds, Is.EqualTo(pausedSeconds));
    }

    /// <summary>
    /// Tests that when the Tick method is called and the remaining seconds reach zero, 
    /// the session state changes to Completed, indicating that the timer has finished counting down.
    /// </summary>
    [Test]
    public void Tick_WhenReachingZero_ShouldChangeToCompleted()
    {
        // Arrange
        _session.Start(1);

        // Act
        for (int i = 0; i < 60; i++)
        {
            _session.Tick();
        }

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Completed));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Tests that calling the Tick method after the session has completed does not decrement the remaining seconds below zero, 
    /// ensuring that the timer does not go into negative values and remains at zero once completed.
    /// </summary>
    [Test]
    public void Tick_AfterCompleted_ShouldNotDecrementBelowZero()
    {
        // Arrange
        _session.Start(1);
        for (int i = 0; i < 60; i++) _session.Tick();

        // Act
        _session.Tick();
        _session.Tick();

        // Assert
        Assert.That(_session.RemainingSeconds, Is.EqualTo(0));
    }

    #endregion

    #region CompletePomodoro Tests
    /// <summary>
    /// Tests that completing a Pomodoro session increments the CompletedPomodoros counter by one and resets the session state to Ready.
    /// </summary>
    [Test]
    public void CompletePomodoro_FromPomodoroSession_ShouldIncrementCounter()
    {
        // Arrange
        _session.Start(25);

        // Act
        _session.CompletePomodoro();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.CompletedPomodoros, Is.EqualTo(1));
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
        });
    }

    /// <summary>
    /// Tests that completing a session while in a break state does not increment the CompletedPomodoros counter and resets the session state to Ready.
    /// </summary>
    [Test]
    public void CompletePomodoro_FromBreakSession_ShouldNotIncrementCounter()
    {
        // Arrange
        _session.StartBreak(SessionType.ShortBreak, 5);
        _session.Start(5);

        // Act
        _session.CompletePomodoro();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.CompletedPomodoros, Is.EqualTo(0));
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
        });
    }

    /// <summary>
    /// Tests that completing multiple Pomodoro sessions correctly increments the CompletedPomodoros counter each time, 
    /// allowing users to track their progress through multiple work intervals.
    /// </summary>
    [Test]
    public void CompletePomodoro_MultipleTimes_ShouldIncrementCorrectly()
    {
        // Act & Assert
        for (int i = 1; i <= 5; i++)
        {
            _session.Start(25);
            _session.CompletePomodoro();
            Assert.That(_session.CompletedPomodoros, Is.EqualTo(i));
        }
    }

    #endregion

    #region StartBreak Tests
    /// <summary>
    /// Tests that starting a short break sets the CurrentSessionType to ShortBreak and changes the state to Break, 
    /// allowing users to take a short rest after completing a Pomodoro session.
    /// </summary>
    [Test]
    public void StartBreak_WithShortBreak_ShouldSetCorrectTypeAndState()
    {
        // Act
        _session.StartBreak(SessionType.ShortBreak, 5);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak));
            Assert.That(_session.State, Is.EqualTo(TimerState.Break));
        });
    }

    /// <summary>
    /// Tests that starting a long break sets the CurrentSessionType to LongBreak and changes the state to Break.
    /// </summary>
    [Test]
    public void StartBreak_WithLongBreak_ShouldSetCorrectTypeAndState()
    {
        // Act
        _session.StartBreak(SessionType.LongBreak, 15);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.LongBreak));
            Assert.That(_session.State, Is.EqualTo(TimerState.Break));
        });
    }

    #endregion

    #region ResetAfterLongBreak Tests
    /// <summary>
    /// Tests that resetting after a long break clears the CompletedPomodoros counter
    /// but does NOT change the CurrentSessionType (we are still IN the long break).
    /// </summary>
    [Test]
    public void ResetAfterLongBreak_ShouldClearCompletedPomodorosButPreserveBreakType()
    {
        // Arrange
        _session.Start(25);
        _session.CompletePomodoro();
        _session.Start(25);
        _session.CompletePomodoro();
        _session.StartBreak(SessionType.LongBreak, 15);

        // Act
        _session.ResetAfterLongBreak();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.CompletedPomodoros, Is.EqualTo(0),
                "Counter should be reset");
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
                "Should still be in LongBreak session");
        });
    }

    #endregion

    #region Integration Tests (Full Flow)
    /// <summary>
    /// Tests a full workflow of starting a Pomodoro session, pausing, resuming, completing the session, and then starting a short break, 
    /// pausing and resuming the break, and finally completing the break.
    /// </summary>
    [Test]
    public void FullWorkflow_PomodoroThenShortBreak_ShouldWorkCorrectly()
    {
        // UC-01: Start Pomodoro
        _session.Start(25);
        Assert.That(_session.State, Is.EqualTo(TimerState.Running));

        // UC-02: Pause
        _session.Pause();
        Assert.That(_session.State, Is.EqualTo(TimerState.Paused));

        // UC-02: Resume
        _session.Resume();
        Assert.That(_session.State, Is.EqualTo(TimerState.Running));

        // Complete Pomodoro
        _session.CompletePomodoro();
        Assert.Multiple(() =>
        {
            Assert.That(_session.CompletedPomodoros, Is.EqualTo(1));
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
        });

        // Start Short Break (NO necesita llamar a Start() después)
        _session.StartBreak(SessionType.ShortBreak, 5);
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Break));
            Assert.That(_session.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak));
        });

        // UC-06: Pause Break
        _session.Pause();
        Assert.That(_session.State, Is.EqualTo(TimerState.Paused));

        // UC-06: Resume Break
        _session.Resume();
        Assert.That(_session.State, Is.EqualTo(TimerState.Break));

        // Complete Break
        _session.CompletePomodoro();
        Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
    }

    [Test]
    public void FullWorkflow_CancelDuringBreak_ShouldReturnToReady()
    {
        // Start and complete Pomodoro
        _session.Start(25);
        _session.CompletePomodoro();

        // Start Break (SIN llamar a Start() después)
        _session.StartBreak(SessionType.ShortBreak, 5);

        // UC-06: Cancel Break
        _session.Cancel();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_session.State, Is.EqualTo(TimerState.Ready));
            Assert.That(_session.RemainingSeconds, Is.EqualTo(0));
        });
    }

    #endregion
}
