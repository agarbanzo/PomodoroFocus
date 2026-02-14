using PomodoroFocus.Application.Services;
using PomodoroFocus.Domain.Enums;
using PomodoroFocus.Domain.ValueObjects;

namespace PomodoroFocus.Tests.Services;

[TestFixture]
public class PomodoroServiceTests
{
    private PomodoroService _service = null!;
    private TimeConfiguration _config = null!;

    [SetUp]
    public void SetUp()
    {
        _config = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        _service = new PomodoroService(_config);
    }

    [TearDown]
    public void TearDown()
    {
        _service?.Dispose();
    }

    #region Initialization Tests

    [Test]
    public void Constructor_WithDefaultConfig_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var service = new PomodoroService();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready));
            Assert.That(service.CurrentSessionType, Is.EqualTo(SessionType.Pomodoro));
            Assert.That(service.CompletedPomodoros, Is.EqualTo(0));
        });
    }

    [Test]
    public void Constructor_WithCustomConfig_ShouldInitializeCorrectly()
    {
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.Pomodoro));
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(0));
        });
    }

    #endregion

    #region UC-01: StartPomodoro Tests

    [Test]
    public void StartPomodoro_ShouldStartWithCorrectDuration()
    {
        // Act
        _service.StartPomodoro();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Running));
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.Pomodoro));
            Assert.That(_service.RemainingSeconds, Is.EqualTo(25 * 60));
        });
    }

    [Test]
    public void StartPomodoro_ShouldTriggerTimerTickEvent()
    {
        // Arrange
        var eventTriggered = false;
        _service.OnTimerTick += () => eventTriggered = true;

        // Act
        _service.StartPomodoro();
        Thread.Sleep(1100); // Wait for at least one tick

        // Assert
        Assert.That(eventTriggered, Is.True);
    }

    #endregion

    #region UC-02: Pause and Resume Tests

    [Test]
    public void Pause_FromRunningPomodoro_ShouldChangeToPaused()
    {
        // Arrange
        _service.StartPomodoro();

        // Act
        _service.Pause();

        // Assert
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Paused));
    }

    [Test]
    public void Resume_FromPausedPomodoro_ShouldReturnToRunning()
    {
        // Arrange
        _service.StartPomodoro();
        _service.Pause();

        // Act
        _service.Resume();

        // Assert
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Running));
    }

    [Test]
    public void PauseAndResume_ShouldPreserveRemainingTime()
    {
        // Arrange
        _service.StartPomodoro();
        Thread.Sleep(2100); // Let it tick twice
        _service.Pause();
        var timeAfterPause = _service.RemainingSeconds;

        // Act
        Thread.Sleep(1000); // Wait while paused
        _service.Resume();

        // Assert
        Assert.That(_service.RemainingSeconds, Is.EqualTo(timeAfterPause)
            .Within(1)); // Allow 1 second tolerance
    }

    #endregion

    #region StartBreak Tests

    [Test]
    public void StartBreak_AfterFirstPomodoro_ShouldStartShortBreak()
    {
        // Arrange
        _service.StartPomodoro();
        _service.CancelAsCompleted();

        // Act
        _service.StartBreak();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Break));
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak));
            Assert.That(_service.RemainingSeconds, Is.EqualTo(5 * 60));
        });
    }

    [Test]
    public void StartBreak_AfterFourthPomodoro_ShouldStartLongBreak()
    {
        // Arrange - Complete 4 Pomodoros
        for (int i = 0; i < 4; i++)
        {
            _service.StartPomodoro();
            _service.CancelAsCompleted();
        }

        // Act
        _service.StartBreak();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Break));
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak));
            Assert.That(_service.RemainingSeconds, Is.EqualTo(15 * 60));
        });
    }

    #endregion

    #region UC-06: Pause, Resume and Cancel Break Tests

    [Test]
    public void Pause_FromBreak_ShouldChangeToPaused()
    {
        // Arrange
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        _service.StartBreak();

        // Act
        _service.Pause();

        // Assert
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Paused));
    }

    [Test]
    public void Resume_FromPausedBreak_ShouldReturnToBreak()
    {
        // Arrange
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        _service.StartBreak();
        _service.Pause();

        // Act
        _service.Resume();

        // Assert
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Break));
    }

    [Test]
    public void CancelAsIncomplete_FromBreak_ShouldReturnToReady()
    {
        // Arrange
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        _service.StartBreak();

        // Act
        _service.CancelAsIncomplete();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
            Assert.That(_service.RemainingSeconds, Is.EqualTo(0));
        });
    }

    #endregion

    #region Cancel Tests

    [Test]
    public void CancelAsCompleted_FromPomodoro_ShouldIncrementCompletedPomodoros()
    {
        // Arrange
        _service.StartPomodoro();

        // Act
        _service.CancelAsCompleted();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(1));
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        });
    }

    [Test]
    public void CancelAsIncomplete_FromPomodoro_ShouldNotIncrementCompletedPomodoros()
    {
        // Arrange
        _service.StartPomodoro();

        // Act
        _service.CancelAsIncomplete();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(0));
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        });
    }

    #endregion

    #region Event Tests

    [Test]
    public void OnSessionComplete_ShouldTriggerWhenSessionCompletes()
    {
        // Arrange
        var eventTriggered = false;

        // Use a very short duration for testing (1 second = almost instant)
        var shortConfig = new TimeConfiguration
        {
            PomodoroDuration = 0, // 0 minutes = 0 seconds
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };

        using var shortService = new PomodoroService(shortConfig);
        shortService.OnSessionComplete += () => eventTriggered = true;

        // Act
        shortService.StartPomodoro();
        Thread.Sleep(1500); // Wait for tick and completion

        // Assert
        Assert.That(eventTriggered, Is.True);
    }

    #endregion

    #region Integration Tests

    [Test]
    public void FullWorkflow_MultiplePomodorosAndBreaks_ShouldWorkCorrectly()
    {
        // Pomodoro 1
        _service.StartPomodoro();
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Running));
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(1));

        // Short Break 1
        _service.StartBreak();
        Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak));
        _service.CancelAsCompleted();

        // Pomodoro 2
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(2));

        // Short Break 2
        _service.StartBreak();
        Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak));
        _service.CancelAsCompleted();

        // Pomodoro 3
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(3));

        // Short Break 3
        _service.StartBreak();
        Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak));
        _service.CancelAsCompleted();

        // Pomodoro 4
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(4));

        // Long Break (after 4 Pomodoros)
        _service.StartBreak();
        Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak));
    }

    #endregion
}
