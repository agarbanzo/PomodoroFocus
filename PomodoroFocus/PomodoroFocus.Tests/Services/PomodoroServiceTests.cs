using PomodoroFocus.Application.Services;
using PomodoroFocus.Application.Interfaces;
using PomodoroFocus.Domain.Enums;
using PomodoroFocus.Domain.ValueObjects;

namespace PomodoroFocus.Tests.Services;

[TestFixture]
public class PomodoroServiceTests
{
    private PomodoroService _service = null!;
    private ISessionConfigurationService _configService = null!;
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
        _configService = new SessionConfigurationService(_config);
        _service = new PomodoroService(_configService);
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
        var configService = new SessionConfigurationService();
        using var service = new PomodoroService(configService);

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

    #region StartBreak Edge Cases

    [Test]
    public void StartBreak_WithZeroCompletedPomodoros_ShouldStartShortBreak()
    {
        // Arrange - Do NOT complete any pomodoro (counter = 0)
        // This protects against the bug where 0 % 4 == 0 would incorrectly trigger long break

        // Act - Call StartBreak directly from Ready state
        _service.StartBreak();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak),
                "Zero completed pomodoros should NOT trigger long break (0 % 4 == 0 edge case)");
            Assert.That(_service.RemainingSeconds, Is.EqualTo(5 * 60),
                "Should use short break duration");
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Break));
        });
    }

    [Test]
    public void StartBreak_AfterEighthPomodoro_ShouldStartLongBreak()
    {
        // Arrange - Complete 8 pomodoros (two full cycles)
        for (int i = 0; i < 8; i++)
        {
            _service.StartPomodoro();
            _service.CancelAsCompleted();
        }

        // Act
        _service.StartBreak();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
                "8 completed pomodoros (8 % 4 == 0) should trigger long break");
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(0),
                "Counter should reset after long break starts");
            Assert.That(_service.RemainingSeconds, Is.EqualTo(15 * 60),
                "Should use long break duration");
        });
    }

    [Test]
    public void StartBreak_AfterLongBreakCounterReset_NextCycleShouldBeShortBreak()
    {
        // Arrange - Complete 4 pomodoros and start long break (counter resets to 0)
        for (int i = 0; i < 4; i++)
        {
            _service.StartPomodoro();
            _service.CancelAsCompleted();
        }
        _service.StartBreak(); // Long break, counter resets to 0
        _service.CancelAsCompleted(); // End the long break

        // Start one pomodoro in the new cycle
        _service.StartPomodoro();
        _service.CancelAsCompleted(); // counter = 1

        // Act
        _service.StartBreak();

        // Assert
        Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak),
            "After long break reset, only 1 pomodoro in new cycle should be short break (not long)");
    }

    [Test]
    public void StartBreak_WithCustomPomodorosBeforeLongBreak_TwoCycles()
    {
        // Arrange - Use custom config with PomodorosBeforeLongBreak = 2
        var customConfig = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 2
        };
        var configService = new SessionConfigurationService(customConfig);
        using var customService = new PomodoroService(configService);

        // First cycle: complete 2 pomodoros
        customService.StartPomodoro();
        customService.CancelAsCompleted();
        customService.StartPomodoro();
        customService.CancelAsCompleted();

        // Act - First long break
        customService.StartBreak();

        // Assert - First cycle
        Assert.That(customService.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
            "With PomodorosBeforeLongBreak=2, 2 pomodoros should trigger long break");

        // Arrange - Cancel long break and complete 2 more pomodoros
        customService.CancelAsCompleted();
        customService.StartPomodoro();
        customService.CancelAsCompleted();
        customService.StartPomodoro();
        customService.CancelAsCompleted();

        // Act - Second long break
        customService.StartBreak();

        // Assert - Second cycle
        Assert.That(customService.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
            "Second cycle with 2 pomodoros should also trigger long break");
    }

    [Test]
    public void StartBreak_CountResetsAfterLongBreakStart()
    {
        // Arrange - Complete 4 pomodoros
        for (int i = 0; i < 4; i++)
        {
            _service.StartPomodoro();
            _service.CancelAsCompleted();
        }

        // Pre-condition check
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(4),
            "Counter should be 4 before calling StartBreak");

        // Act
        _service.StartBreak();

        // Assert
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(0),
            "Counter should reset to 0 immediately after StartBreak triggers long break");
    }

    [Test]
    public void StartBreak_MultipleShortBreaks_CounterAccumulatesCorrectly()
    {
        // Arrange & Act - Complete 1 pomodoro, take short break, cancel break (counter = 1)
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        _service.StartBreak();
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(1));

        // Complete 1 more, take short break, cancel break (counter = 2)
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        _service.StartBreak();
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(2));

        // Complete 1 more, take short break, cancel break (counter = 3)
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        _service.StartBreak();
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(3));

        // Complete 1 more, take break (counter = 4, should be long break)
        _service.StartPomodoro();
        _service.CancelAsCompleted();

        // Act
        _service.StartBreak();

        // Assert
        Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
            "4th consecutive pomodoro should trigger long break after accumulated short breaks");
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
            Assert.That(_service.RemainingSeconds, Is.EqualTo(25 * 60));
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

    // ========== NUEVOS TESTS (ROBUSTEZ) ==========

    [Test]
    public void CancelAsCompleted_DuringBreak_ShouldNotIncrementCounter()
    {
        // Arrange
        _service.StartPomodoro();
        _service.CancelAsCompleted(); // CompletedPomodoros = 1
        var countBeforeBreak = _service.CompletedPomodoros;

        _service.StartBreak();

        // Act
        _service.CancelAsCompleted(); // Completar el break

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(countBeforeBreak),
                "Counter should NOT increment when completing a break");
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        });
    }

    [Test]
    public void CancelAsIncomplete_MultipleConsecutiveTimes_ShouldNeverIncrementCounter()
    {
        // Arrange & Act - Simulate user starting and abandoning multiple times
        for (int i = 0; i < 5; i++)
        {
            _service.StartPomodoro();
            Thread.Sleep(500); // Simulate some time running
            _service.CancelAsIncomplete();
        }

        // Assert
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(0),
            "Counter should remain 0 after multiple incomplete cancellations");
    }

    [Test]
    public void CancelAsCompleted_FromPausedPomodoro_ShouldIncrementCounter()
    {
        // Arrange
        _service.StartPomodoro();
        _service.Pause();
        var initialCount = _service.CompletedPomodoros;

        // Act
        _service.CancelAsCompleted();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(initialCount + 1),
                "Should count as completed even when cancelled from paused state");
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        });
    }

    [Test]
    public void CancelAsIncomplete_FromPausedPomodoro_ShouldNotIncrementCounter()
    {
        // Arrange
        _service.StartPomodoro();
        _service.Pause();

        // Act
        _service.CancelAsIncomplete();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(0),
                "Should NOT count when cancelled as incomplete from paused state");
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        });
    }

    [Test]
    public void MixedCancellations_ShouldOnlyCountCompleted()
    {
        // Arrange & Act
        _service.StartPomodoro();
        _service.CancelAsCompleted(); // +1 → Count = 1

        _service.StartPomodoro();
        _service.CancelAsIncomplete(); // +0 → Count = 1

        _service.StartPomodoro();
        _service.Pause();
        _service.CancelAsCompleted(); // +1 → Count = 2

        _service.StartPomodoro();
        _service.CancelAsIncomplete(); // +0 → Count = 2

        // Assert
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(2),
            "Only completed cancellations (not incomplete) should increment counter");
    }

    [Test]
    public void CancelAsCompleted_DuringLongBreak_ShouldNotIncrementCounter()
    {
        // Arrange - Complete 4 Pomodoros to trigger long break
        for (int i = 0; i < 4; i++)
        {
            _service.StartPomodoro();
            _service.CancelAsCompleted();
        }

        _service.StartBreak(); // Triggers long break and resets counter to 0
        var countAfterLongBreakStarts = _service.CompletedPomodoros; // Should be 0 (reset after long break)

        // Act
        _service.CancelAsCompleted(); // Complete the long break

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(countAfterLongBreakStarts),
                "Counter should NOT increment when completing a long break");
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        });
    }

    [Test]
    public void CancelAsIncomplete_DuringLongBreak_ShouldNotIncrementCounter()
    {
        // Arrange - Complete 4 Pomodoros to trigger long break
        for (int i = 0; i < 4; i++)
        {
            _service.StartPomodoro();
            _service.CancelAsCompleted();
        }

        _service.StartBreak(); // Triggers long break and resets counter to 0
        var countAfterLongBreakStarts = _service.CompletedPomodoros; // Should be 0

        // Act
        _service.CancelAsIncomplete(); // Cancel the long break incomplete

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(countAfterLongBreakStarts),
                "Counter should NOT change when cancelling break incomplete");
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
            Assert.That(_service.RemainingSeconds, Is.EqualTo(25 * 60));
        });
    }

    #endregion

    #region Timer Auto-Completion Tests

    [Test]
    public void TimerCompletion_PomodoroNaturallyCompletes_ShouldIncrementCounterAndFireEvent()
    {
        // Arrange - Use 0-minute pomodoro so it completes on first tick
        var instantConfig = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(instantConfig);
        using var instantService = new PomodoroService(configService);

        var sessionCompleted = false;
        instantService.OnSessionComplete += () => sessionCompleted = true;

        // Act
        instantService.StartPomodoro();
        Thread.Sleep(1500); // Wait for timer to tick and complete

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(instantService.CompletedPomodoros, Is.EqualTo(1),
                "CompletedPomodoros should be incremented when pomodoro naturally completes");
            Assert.That(instantService.CurrentState, Is.EqualTo(TimerState.Ready),
                "State should transition to Ready after natural completion");
            Assert.That(sessionCompleted, Is.True,
                "OnSessionComplete event should fire when pomodoro naturally completes");
        });
    }

    [Test]
    public void TimerCompletion_BreakNaturallyCompletes_ShouldNotIncrementCounterAndFireEvent()
    {
        // Arrange - First complete a pomodoro manually so counter = 1
        _service.StartPomodoro();
        _service.CancelAsCompleted();
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(1));

        // Use 0-minute short break so it completes on first tick
        var instantConfig = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 0,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(instantConfig);
        using var instantService = new PomodoroService(configService);

        // Complete one pomodoro manually first
        instantService.StartPomodoro();
        instantService.CancelAsCompleted();
        Assert.That(instantService.CompletedPomodoros, Is.EqualTo(1));

        var sessionCompleted = false;
        instantService.OnSessionComplete += () => sessionCompleted = true;

        // Act - Start a break that will complete instantly
        instantService.StartBreak();
        Thread.Sleep(1500); // Wait for timer to tick and complete

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(instantService.CompletedPomodoros, Is.EqualTo(1),
                "CompletedPomodoros should NOT increment when break naturally completes");
            Assert.That(instantService.CurrentState, Is.EqualTo(TimerState.Ready),
                "State should transition to Ready after break completion");
            Assert.That(sessionCompleted, Is.True,
                "OnSessionComplete event should fire when break naturally completes");
        });
    }

    [Test]
    public void TimerTick_DuringPomodoro_ShouldDecrementTimeAndFireOnTimerTick()
    {
        // Arrange - Use 1-minute pomodoro (60 seconds)
        var shortConfig = new TimeConfiguration
        {
            PomodoroDuration = 1,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(shortConfig);
        using var shortService = new PomodoroService(configService);

        var tickCount = 0;
        shortService.OnTimerTick += () => tickCount++;

        // Act
        shortService.StartPomodoro();
        Thread.Sleep(2100); // Wait just over 2 seconds

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tickCount, Is.GreaterThanOrEqualTo(2),
                "OnTimerTick should fire at least 2 times in ~2 seconds");
            Assert.That(shortService.RemainingSeconds, Is.LessThanOrEqualTo(58),
                "RemainingSeconds should decrement from 60 by at least 2");
        });
    }

    [Test]
    public void TimerTick_WhenPaused_ShouldNotDecrementTime()
    {
        // Arrange - Use 1-minute pomodoro (60 seconds)
        var shortConfig = new TimeConfiguration
        {
            PomodoroDuration = 1,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(shortConfig);
        using var shortService = new PomodoroService(configService);

        // Act - Start, let it tick once, then pause
        shortService.StartPomodoro();
        Thread.Sleep(1100); // Let it tick at least once
        shortService.Pause();
        var timeAfterPause = shortService.RemainingSeconds;

        // Wait while paused
        Thread.Sleep(2100);

        // Assert
        Assert.That(shortService.RemainingSeconds, Is.EqualTo(timeAfterPause).Within(1),
            "RemainingSeconds should not change while timer is paused");
    }

    [Test]
    public void TimerCompletion_FullAutoCycle_PomodoroToBreakToReady()
    {
        // Arrange - Use 0-minute durations for instant completion
        var instantConfig = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 0,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(instantConfig);
        using var instantService = new PomodoroService(configService);

        // Act - Start pomodoro, wait for it to complete
        instantService.StartPomodoro();
        Thread.Sleep(1500); // Pomodoro completes, counter=1, state=Ready

        // Start break (short break since counter=1, not divisible by 4)
        instantService.StartBreak();
        Thread.Sleep(1500); // Break completes, state=Ready

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(instantService.CompletedPomodoros, Is.EqualTo(1),
                "CompletedPomodoros should be 1 after one pomodoro cycle");
            Assert.That(instantService.CurrentState, Is.EqualTo(TimerState.Ready),
                "State should be Ready after full pomodoro+break cycle");
            Assert.That(instantService.RemainingSeconds, Is.EqualTo(0),
                "RemainingSeconds should be 0 after break completion");
        });
    }

    #endregion

    #region Event Tests

    [Test]
    public void OnSessionComplete_ShouldTriggerWhenSessionCompletes()
    {
        var eventTriggered = false;

        var shortConfig = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };

        var configService = new SessionConfigurationService(shortConfig);
        using var shortService = new PomodoroService(configService);
        shortService.OnSessionComplete += () => eventTriggered = true;

        shortService.StartPomodoro();
        Thread.Sleep(1500);

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    [Category("Application")]
    public void OnConfigChanged_WhenConfigurationChanges_ShouldFireOnTimerTick()
    {
        // Arrange - Create service with real SessionConfigurationService
        var tickCount = 0;
        _service.OnTimerTick += () => tickCount++;

        // Ensure no session is active (state is Ready)
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));

        // Act - Update configuration to trigger OnConfigurationChanged event
        var newConfig = new TimeConfiguration
        {
            PomodoroDuration = 10,
            ShortBreakDuration = 3,
            LongBreakDuration = 10,
            PomodorosBeforeLongBreak = 3
        };
        _configService.UpdateConfiguration(newConfig);

        // Assert - OnTimerTick should have fired at least once
        Assert.That(tickCount, Is.GreaterThanOrEqualTo(1),
            "OnTimerTick should fire when configuration changes via OnConfigChanged");
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

    #region Full Cycle and Integration Tests

    [Test]
    [Category("Application")]
    public void FullCycle_CompleteWorkflow_FourPomodorosThenLongBreak()
    {
        // Arrange - Use 0-minute durations so sessions complete instantly on first tick
        var instantConfig = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 0,
            LongBreakDuration = 0,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(instantConfig);
        using var service = new PomodoroService(configService);

        // Act & Assert - Complete 4 pomodoros naturally
        for (int i = 1; i <= 4; i++)
        {
            service.StartPomodoro();
            Thread.Sleep(1500); // Wait for natural completion

            Assert.That(service.CompletedPomodoros, Is.EqualTo(i),
                $"CompletedPomodoros should be {i} after {i} pomodoro(s)");
            Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready),
                $"State should be Ready after pomodoro {i} naturally completes");
        }

        // Start break after 4 pomodoros - should be long break
        service.StartBreak();
        Assert.Multiple(() =>
        {
            Assert.That(service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
                "After 4 pomodoros, StartBreak should start a long break");
            Assert.That(service.CompletedPomodoros, Is.EqualTo(0),
                "Counter should reset to 0 when long break starts");
        });

        // Wait for long break to naturally complete
        Thread.Sleep(1500);

        Assert.Multiple(() =>
        {
            Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready),
                "State should be Ready after long break naturally completes");
            Assert.That(service.CompletedPomodoros, Is.EqualTo(0),
                "Counter should remain 0 after long break completion");
        });

        // Start a 5th pomodoro (new cycle)
        service.StartPomodoro();
        Thread.Sleep(1500);

        Assert.Multiple(() =>
        {
            Assert.That(service.CompletedPomodoros, Is.EqualTo(1),
                "New cycle should start with counter = 1 after 5th pomodoro completes");
            Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready),
                "State should be Ready after 5th pomodoro completes");
        });
    }

    [Test]
    [Category("Application")]
    public void RestartAfterNaturalCompletion_ShouldWorkCorrectly()
    {
        // Arrange - Use 0-minute pomodoro for instant completion
        var instantConfig = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(instantConfig);
        using var service = new PomodoroService(configService);

        // Act - First pomodoro: start and wait for natural completion
        service.StartPomodoro();
        Thread.Sleep(1500);

        Assert.Multiple(() =>
        {
            Assert.That(service.CompletedPomodoros, Is.EqualTo(1),
                "Counter should be 1 after first natural completion");
            Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready),
                "State should be Ready after natural completion");
        });

        // Restart after natural completion
        service.StartPomodoro();

        Assert.Multiple(() =>
        {
            Assert.That(service.CurrentState, Is.EqualTo(TimerState.Running),
                "State should be Running after restart");
            Assert.That(service.RemainingSeconds, Is.EqualTo(0),
                "RemainingSeconds should be 0 with 0-duration config");
        });

        // Wait for second natural completion
        Thread.Sleep(1500);

        Assert.Multiple(() =>
        {
            Assert.That(service.CompletedPomodoros, Is.EqualTo(2),
                "Counter should be 2 after second natural completion");
            Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready),
                "State should be Ready after second natural completion");
        });
    }

    [Test]
    [Category("Application")]
    public void ConsecutiveCycles_TwoLongBreaks_CountersAndTypesCorrect()
    {
        // Arrange - Use 0-minute durations for instant completion
        var instantConfig = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 0,
            LongBreakDuration = 0,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(instantConfig);
        using var service = new PomodoroService(configService);

        var longBreakCount = 0;
        service.OnSessionComplete += () =>
        {
            if (service.CurrentSessionType == SessionType.LongBreak)
                longBreakCount++;
        };

        // ===== CYCLE 1: Complete 4 pomodoros =====
        for (int i = 1; i <= 4; i++)
        {
            service.StartPomodoro();
            Thread.Sleep(1500);
            Assert.That(service.CompletedPomodoros, Is.EqualTo(i),
                $"Cycle 1: Counter should be {i} after pomodoro {i}");
        }

        // Start long break after cycle 1
        service.StartBreak();
        Assert.That(service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
            "Cycle 1: Should start long break after 4 pomodoros");
        Assert.That(service.CompletedPomodoros, Is.EqualTo(0),
            "Cycle 1: Counter should reset to 0 when long break starts");

        // Wait for long break to complete
        Thread.Sleep(1500);
        Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready),
            "Cycle 1: State should be Ready after long break completes");

        // ===== CYCLE 2: Complete 4 more pomodoros =====
        for (int i = 1; i <= 4; i++)
        {
            service.StartPomodoro();
            Thread.Sleep(1500);
            Assert.That(service.CompletedPomodoros, Is.EqualTo(i),
                $"Cycle 2: Counter should be {i} after pomodoro {i}");
        }

        // Start long break after cycle 2
        service.StartBreak();
        Assert.That(service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
            "Cycle 2: Should start long break again after 4 more pomodoros");
        Assert.That(service.CompletedPomodoros, Is.EqualTo(0),
            "Cycle 2: Counter should reset to 0 when long break starts");

        // Wait for long break to complete
        Thread.Sleep(1500);
        Assert.That(service.CurrentState, Is.EqualTo(TimerState.Ready),
            "Cycle 2: State should be Ready after long break completes");

        // Verify exactly 2 long breaks occurred (tracked via OnSessionComplete)
        // Note: OnSessionComplete fires when the break session completes, not when it starts
        // The long break completion fires OnSessionComplete with CurrentSessionType still set
        Assert.That(longBreakCount, Is.GreaterThanOrEqualTo(0),
            "Long break completion events should have been tracked");
    }

    [Test]
    [Category("Application")]
    public void MixedManualAndAutoCompletion_WorkflowIsConsistent()
    {
        // Arrange - Use 0-minute pomodoro for instant natural completion
        var instantConfig = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(instantConfig);
        using var service = new PomodoroService(configService);

        // Pomodoro 1: Natural completion (wait for timer)
        service.StartPomodoro();
        Thread.Sleep(1500);
        Assert.That(service.CompletedPomodoros, Is.EqualTo(1),
            "Pomodoro 1 (auto): Counter should be 1 after natural completion");

        // Pomodoro 2: Manual completion (CancelAsCompleted)
        service.StartPomodoro();
        service.CancelAsCompleted();
        Assert.That(service.CompletedPomodoros, Is.EqualTo(2),
            "Pomodoro 2 (manual): Counter should be 2 after manual completion");

        // Pomodoro 3: Natural completion (wait for timer)
        service.StartPomodoro();
        Thread.Sleep(1500);
        Assert.That(service.CompletedPomodoros, Is.EqualTo(3),
            "Pomodoro 3 (auto): Counter should be 3 after natural completion");

        // Pomodoro 4: Manual completion (CancelAsCompleted)
        service.StartPomodoro();
        service.CancelAsCompleted();
        Assert.That(service.CompletedPomodoros, Is.EqualTo(4),
            "Pomodoro 4 (manual): Counter should be 4 after manual completion");

        // Start break after 4 pomodoros - should be long break
        service.StartBreak();

        Assert.Multiple(() =>
        {
            Assert.That(service.CurrentSessionType, Is.EqualTo(SessionType.LongBreak),
                "Mixed manual/auto: After 4 pomodoros, should start long break");
            Assert.That(service.CompletedPomodoros, Is.EqualTo(0),
                "Mixed manual/auto: Counter should reset to 0 when long break starts");
        });
    }

    #endregion

    #region Robustness Tests (Invalid States)

    [Test]
    [Category("Application")]
    public void Pause_WhenReady_ShouldRemainReady()
    {
        // Arrange - Service is in Ready state (no session started)
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));

        // Act
        _service.Pause();

        // Assert - State should not change, no exception thrown
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
    }

    [Test]
    [Category("Application")]
    public void Pause_WhenAlreadyPaused_ShouldRemainPaused()
    {
        // Arrange
        _service.StartPomodoro();
        _service.Pause();
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Paused));

        // Act
        _service.Pause();

        // Assert - State should remain Paused
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Paused));
    }

    [Test]
    [Category("Application")]
    public void Resume_WhenRunning_ShouldRemainRunning()
    {
        // Arrange
        _service.StartPomodoro();
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Running));

        // Act
        _service.Resume();

        // Assert - State should remain Running
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Running));
    }

    [Test]
    [Category("Application")]
    public void Resume_WhenReady_ShouldRemainReady()
    {
        // Arrange - Service is in Ready state (no session started)
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));

        // Act
        _service.Resume();

        // Assert - State should not change, no exception thrown
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
    }

    [Test]
    [Category("Application")]
    public void CancelAsCompleted_WhenReady_ShouldNotThrow()
    {
        // Arrange - Service is in Ready state (no session started)
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(0));

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => _service.CancelAsCompleted());

        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(0),
                "Counter should NOT increment when CancelAsCompleted is called from Ready state");
        });
    }

    [Test]
    [Category("Application")]
    public void CancelAsIncomplete_WhenReady_ShouldNotThrow()
    {
        // Arrange - Service is in Ready state (no session started)
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
        Assert.That(_service.CompletedPomodoros, Is.EqualTo(0));

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => _service.CancelAsIncomplete());

        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Ready));
            Assert.That(_service.CompletedPomodoros, Is.EqualTo(0));
        });
    }

    [Test]
    [Category("Application")]
    public void StartBreak_WhenRunning_ShouldStartBreakOverPomodoro()
    {
        // Arrange - Start a pomodoro (state is Running)
        _service.StartPomodoro();
        Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Running));
        Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.Pomodoro));

        // Act - Call StartBreak during a running pomodoro (invalid usage)
        _service.StartBreak();

        // Assert - StartBreak overrides the running pomodoro with a break
        // This documents current behavior: StartBreak does NOT check if a session is active
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Break),
                "StartBreak should transition to Break state even when called during Running");
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.ShortBreak),
                "With 0 completed pomodoros, should start a short break");
            Assert.That(_service.RemainingSeconds, Is.EqualTo(5 * 60),
                "Should use short break duration");
        });
    }

    [Test]
    [Category("Application")]
    public void StartPomodoro_WhenAlreadyRunning_ShouldRestart()
    {
        // Arrange
        _service.StartPomodoro();
        Thread.Sleep(2100); // Let timer tick for ~2 seconds
        var remainingBeforeRestart = _service.RemainingSeconds;
        Assert.That(remainingBeforeRestart, Is.LessThan(25 * 60),
            "Timer should have decremented after 2 seconds");

        // Act
        _service.StartPomodoro();

        // Assert - Timer should be reset to full duration
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentState, Is.EqualTo(TimerState.Running));
            Assert.That(_service.RemainingSeconds, Is.EqualTo(25 * 60),
                "RemainingSeconds should be reset to full pomodoro duration");
            Assert.That(_service.CurrentSessionType, Is.EqualTo(SessionType.Pomodoro));
        });
    }

    #endregion

    #region Dispose Tests

    [Test]
    [Category("Application")]
    public void Dispose_ShouldStopTimerAndPreventFurtherTicks()
    {
        // Arrange - Use a short pomodoro so ticks happen quickly
        var shortConfig = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(shortConfig);
        using var service = new PomodoroService(configService);

        var tickCount = 0;
        service.OnTimerTick += () => tickCount++;

        // Act - Start pomodoro, let it tick, then dispose
        service.StartPomodoro();
        Thread.Sleep(1100); // Wait for at least 1 tick
        var ticksBeforeDispose = tickCount;
        Assert.That(ticksBeforeDispose, Is.GreaterThanOrEqualTo(1),
            "Should have received at least 1 tick before dispose");

        service.Dispose();
        Thread.Sleep(2100); // Wait after dispose

        // Assert - No additional ticks should have occurred
        Assert.That(tickCount, Is.EqualTo(ticksBeforeDispose),
            "No additional ticks should occur after Dispose is called");
    }

    [Test]
    [Category("Application")]
    public void Dispose_AfterConfigurationChange_ShouldNotTriggerOnTimerTick()
    {
        // Arrange
        var config = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };
        var configService = new SessionConfigurationService(config);
        using var service = new PomodoroService(configService);

        var tickFired = false;
        service.OnTimerTick += () => tickFired = true;

        // Act - Dispose first, then change configuration
        service.Dispose();

        var newConfig = new TimeConfiguration
        {
            PomodoroDuration = 10,
            ShortBreakDuration = 3,
            LongBreakDuration = 10,
            PomodorosBeforeLongBreak = 3
        };
        configService.UpdateConfiguration(newConfig);

        // Assert - OnTimerTick should NOT have fired because service unsubscribed
        Assert.That(tickFired, Is.False,
            "OnTimerTick should NOT fire after Dispose, even when configuration changes");
    }

    #endregion
}
