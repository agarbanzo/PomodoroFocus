using PomodoroFocus.Application.Services;
using PomodoroFocus.Domain.ValueObjects;

namespace PomodoroFocus.Tests.Services;

[TestFixture]
public class SessionConfigurationServiceTests
{
    private SessionConfigurationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new SessionConfigurationService();
    }

    [Test]
    public void Constructor_WithDefaultConfig_ShouldInitializeCorrectly()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentConfiguration.PomodoroDuration, Is.EqualTo(25));
            Assert.That(_service.CurrentConfiguration.ShortBreakDuration, Is.EqualTo(5));
            Assert.That(_service.CurrentConfiguration.LongBreakDuration, Is.EqualTo(15));
            Assert.That(_service.CurrentConfiguration.PomodorosBeforeLongBreak, Is.EqualTo(4));
        });
    }

    [Test]
    public void Constructor_WithCustomConfig_ShouldUseProvidedValues()
    {
        var customConfig = new TimeConfiguration
        {
            PomodoroDuration = 30,
            ShortBreakDuration = 10,
            LongBreakDuration = 20,
            PomodorosBeforeLongBreak = 2
        };

        var service = new SessionConfigurationService(customConfig);

        Assert.Multiple(() =>
        {
            Assert.That(service.CurrentConfiguration.PomodoroDuration, Is.EqualTo(30));
            Assert.That(service.CurrentConfiguration.ShortBreakDuration, Is.EqualTo(10));
            Assert.That(service.CurrentConfiguration.LongBreakDuration, Is.EqualTo(20));
            Assert.That(service.CurrentConfiguration.PomodorosBeforeLongBreak, Is.EqualTo(2));
        });
    }

    [Test]
    public void UpdateConfiguration_ShouldChangeConfiguration()
    {
        var newConfig = new TimeConfiguration
        {
            PomodoroDuration = 15,
            ShortBreakDuration = 3,
            LongBreakDuration = 10,
            PomodorosBeforeLongBreak = 2
        };

        _service.UpdateConfiguration(newConfig);

        Assert.Multiple(() =>
        {
            Assert.That(_service.CurrentConfiguration.PomodoroDuration, Is.EqualTo(15));
            Assert.That(_service.CurrentConfiguration.ShortBreakDuration, Is.EqualTo(3));
            Assert.That(_service.CurrentConfiguration.LongBreakDuration, Is.EqualTo(10));
            Assert.That(_service.CurrentConfiguration.PomodorosBeforeLongBreak, Is.EqualTo(2));
        });
    }

    [Test]
    public void UpdateConfiguration_ShouldTriggerOnConfigurationChangedEvent()
    {
        var eventTriggered = false;
        _service.OnConfigurationChanged += () => eventTriggered = true;

        var newConfig = new TimeConfiguration { PomodoroDuration = 20 };

        _service.UpdateConfiguration(newConfig);

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void OnConfigurationChanged_ShouldAllowMultipleSubscribers()
    {
        var callCount = 0;
        _service.OnConfigurationChanged += () => callCount++;
        _service.OnConfigurationChanged += () => callCount++;

        _service.UpdateConfiguration(new TimeConfiguration { PomodoroDuration = 20 });

        Assert.That(callCount, Is.EqualTo(2));
    }
}