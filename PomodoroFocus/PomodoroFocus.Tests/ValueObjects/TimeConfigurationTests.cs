using NUnit.Framework;
using PomodoroFocus.Domain.ValueObjects;

namespace PomodoroFocus.Tests.ValueObjects;

[TestFixture]
public class TimeConfigurationTests
{
    #region Initialization Tests

    [Test]
    public void DefaultValues_ShouldMatchStandardPomodoroTechnique()
    {
        // Act
        var config = new TimeConfiguration();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.EqualTo(25));
            Assert.That(config.ShortBreakDuration, Is.EqualTo(5));
            Assert.That(config.LongBreakDuration, Is.EqualTo(15));
            Assert.That(config.PomodorosBeforeLongBreak, Is.EqualTo(4));
        });
    }

    [Test]
    public void CustomValues_ShouldBeSetCorrectly()
    {
        // Act
        var config = new TimeConfiguration
        {
            PomodoroDuration = 30,
            ShortBreakDuration = 10,
            LongBreakDuration = 20,
            PomodorosBeforeLongBreak = 3
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.EqualTo(30));
            Assert.That(config.ShortBreakDuration, Is.EqualTo(10));
            Assert.That(config.LongBreakDuration, Is.EqualTo(20));
            Assert.That(config.PomodorosBeforeLongBreak, Is.EqualTo(3));
        });
    }

    [Test]
    public void PartialCustomization_ShouldKeepDefaultsForUnspecifiedValues()
    {
        // Act
        var config = new TimeConfiguration
        {
            PomodoroDuration = 50 // Only customize this
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.EqualTo(50));
            Assert.That(config.ShortBreakDuration, Is.EqualTo(5)); // Default
            Assert.That(config.LongBreakDuration, Is.EqualTo(15)); // Default
            Assert.That(config.PomodorosBeforeLongBreak, Is.EqualTo(4)); // Default
        });
    }

    #endregion

    #region Equality Tests (Record behavior)

    [Test]
    public void TwoConfigs_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var config1 = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };

        var config2 = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            PomodorosBeforeLongBreak = 4
        };

        // Assert
        Assert.That(config1, Is.EqualTo(config2));
    }

    [Test]
    public void TwoConfigs_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var config1 = new TimeConfiguration
        {
            PomodoroDuration = 25
        };

        var config2 = new TimeConfiguration
        {
            PomodoroDuration = 30
        };

        // Assert
        Assert.That(config1, Is.Not.EqualTo(config2));
    }

    [Test]
    public void GetHashCode_ForEqualConfigs_ShouldBeEqual()
    {
        // Arrange
        var config1 = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5
        };

        var config2 = new TimeConfiguration
        {
            PomodoroDuration = 25,
            ShortBreakDuration = 5
        };

        // Assert
        Assert.That(config1.GetHashCode(), Is.EqualTo(config2.GetHashCode()));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void ZeroValues_ShouldBeAllowed()
    {
        // Act
        var config = new TimeConfiguration
        {
            PomodoroDuration = 0,
            ShortBreakDuration = 0,
            LongBreakDuration = 0,
            PomodorosBeforeLongBreak = 0
        };

        // Assert - Records don't have validation by default
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.EqualTo(0));
            Assert.That(config.ShortBreakDuration, Is.EqualTo(0));
            Assert.That(config.LongBreakDuration, Is.EqualTo(0));
            Assert.That(config.PomodorosBeforeLongBreak, Is.EqualTo(0));
        });
    }

    [Test]
    public void NegativeValues_ShouldBeAllowed()
    {
        // Act - Records don't prevent negative values by default
        var config = new TimeConfiguration
        {
            PomodoroDuration = -1,
            ShortBreakDuration = -5,
            LongBreakDuration = -10,
            PomodorosBeforeLongBreak = -2
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.EqualTo(-1));
            Assert.That(config.ShortBreakDuration, Is.EqualTo(-5));
            Assert.That(config.LongBreakDuration, Is.EqualTo(-10));
            Assert.That(config.PomodorosBeforeLongBreak, Is.EqualTo(-2));
        });
    }

    [Test]
    public void VeryLargeValues_ShouldBeAllowed()
    {
        // Act
        var config = new TimeConfiguration
        {
            PomodoroDuration = int.MaxValue,
            ShortBreakDuration = 1000,
            LongBreakDuration = 5000,
            PomodorosBeforeLongBreak = 100
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.EqualTo(int.MaxValue));
            Assert.That(config.ShortBreakDuration, Is.EqualTo(1000));
            Assert.That(config.LongBreakDuration, Is.EqualTo(5000));
            Assert.That(config.PomodorosBeforeLongBreak, Is.EqualTo(100));
        });
    }

    #endregion

    #region Immutability Tests

    [Test]
    public void Init_Properties_ShouldNotAllowModificationAfterInitialization()
    {
        // Arrange
        var config = new TimeConfiguration
        {
            PomodoroDuration = 25
        };

        // Act & Assert - This should not compile if uncommented
        // config.PomodoroDuration = 30; // ❌ CS8852: Init-only property can only be assigned in an object initializer

        // Just verify the value is what we set
        Assert.That(config.PomodoroDuration, Is.EqualTo(25));
    }

    #endregion

    #region Common Configuration Scenarios

    [Test]
    public void ShortPomodoro_Configuration_ShouldWork()
    {
        // Act - For people who prefer shorter sessions
        var config = new TimeConfiguration
        {
            PomodoroDuration = 15,
            ShortBreakDuration = 3,
            LongBreakDuration = 10,
            PomodorosBeforeLongBreak = 6
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.LessThan(25));
            Assert.That(config.PomodorosBeforeLongBreak, Is.GreaterThan(4));
        });
    }

    [Test]
    public void LongPomodoro_Configuration_ShouldWork()
    {
        // Act - For deep work sessions
        var config = new TimeConfiguration
        {
            PomodoroDuration = 50,
            ShortBreakDuration = 10,
            LongBreakDuration = 30,
            PomodorosBeforeLongBreak = 2
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.PomodoroDuration, Is.GreaterThan(25));
            Assert.That(config.LongBreakDuration, Is.GreaterThan(15));
        });
    }

    #endregion
}
