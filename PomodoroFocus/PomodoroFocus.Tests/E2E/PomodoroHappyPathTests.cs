using PomodoroFocus.Tests.Fixtures;

namespace PomodoroFocus.Tests.E2E;

/// <summary>
/// E2E tests for the PomodoroFocus "Happy Path" user flow.
/// Tests the main scenario: Start → Pause → Resume → Complete → Break → Cancel
/// 
/// Use Cases covered:
/// - UC-07: Load the page
/// - UC-01: Start a Pomodoro
/// - UC-02: Pause and Resume
/// - UC-04: Complete a Pomodoro
/// - UC-05: Start a short break
/// - UC-06: Cancel a break
/// </summary>
[TestFixture]
[Category("E2E")]
[Category("HappyPath")]
public class PomodoroHappyPathTests : PlaywrightFixture
{
    private const string BaseUrl = "http://localhost:5294";
    
    // UI Selectors
    private const string TimerDisplaySelector = ".timer-text-overlay";
    private const string StartButtonSelector = "button:has-text(\"Iniciar Pomodoro\")";
    private const string PauseButtonSelector = "button:has-text(\"Pausar\")";
    private const string ResumeButtonSelector = "button:has-text(\"Reanudar\")";
    // Use specific selector for Cancel button in timer controls (btn-danger class) to avoid conflict with SettingsModal's Cancel button
    private const string CancelButtonSelector = ".timer-controls button.btn-danger:has-text(\"Cancelar\")";
    private const string ModalBackdropSelector = ".modal-backdrop.visible";
    private const string ConfirmCompletedButtonSelector = "button:has-text(\"Sí, marcar como completado\")";
    private const string ConfirmNotCompletedButtonSelector = "button:has-text(\"No, descartar sesión\")";
    private const string ShortBreakButtonSelector = "button:has-text(\"Iniciar Descanso Corto\")";

    [OneTimeSetUp]
    public async Task OneTimeSetupAsync()
    {
        await InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTeardownAsync()
    {
        await DisposeAsync();
    }

    [Test]
    [Order(1)]
    [Description("UC-07: Verify the main Pomodoro page loads correctly")]
    public async Task LoadPage_ShouldDisplayTimerAndStartButton()
    {
        // Act - Navigate to the page
        await NavigateToAsync(BaseUrl);

        // Assert - Verify key UI elements are present
        Assert.Multiple(async () =>
        {
            // Timer display should be visible
            await WaitForElementAsync(TimerDisplaySelector);
            var timerText = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            Assert.That(timerText, Is.EqualTo("25:00"), "Timer should display 25:00 initially");

            // Start button should be visible
            await WaitForElementAsync(StartButtonSelector);
            var startButton = Page.Locator(StartButtonSelector);
            Assert.That(await startButton.IsVisibleAsync(), Is.True, "Start button should be visible");

            // Verify no Blazor errors
            Assert.That(await HasBlazorErrorAsync(), Is.False, "Should not have Blazor errors");
        });

        TestContext.Out.WriteLine("✓ Page loaded successfully with timer at 25:00 and Start button visible");
    }

    [Test]
    [Order(2)]
    [Description("UC-01: Start a Pomodoro session")]
    public async Task StartPomodoro_ShouldChangeTimerToRunning()
    {
        // Arrange - Ensure we're on the page
        await NavigateToAsync(BaseUrl);

        // Act - Click Start button
        await ClickAndWaitAsync(StartButtonSelector);

        // Assert - Verify timer is running
        Assert.Multiple(async () =>
        {
            // Pause button should now be visible (instead of Start)
            await WaitForElementAsync(PauseButtonSelector);
            var pauseButton = Page.Locator(PauseButtonSelector);
            Assert.That(await pauseButton.IsVisibleAsync(), Is.True, "Pause button should be visible when running");

            // Cancel button should be visible
            await WaitForElementAsync(CancelButtonSelector);
            var cancelButton = Page.Locator(CancelButtonSelector);
            Assert.That(await cancelButton.IsVisibleAsync(), Is.True, "Cancel button should be visible when running");

            // Timer should start counting down (wait a bit and verify it changed)
            var initialTime = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            await Task.Delay(1500);
            var timeAfterDelay = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            
            Assert.That(timeAfterDelay, Is.Not.EqualTo(initialTime), 
                "Timer should decrement when running");
        });

        TestContext.Out.WriteLine("✓ Pomodoro started successfully - timer is running");
    }

    [Test]
    [Order(3)]
    [Description("UC-02: Pause a running Pomodoro")]
    public async Task PausePomodoro_ShouldChangeToPausedState()
    {
        // Arrange - Start a Pomodoro first
        await NavigateToAsync(BaseUrl);
        await ClickAndWaitAsync(StartButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);

        // Act - Click Pause button
        await ClickAndWaitAsync(PauseButtonSelector);

        // Assert - Verify timer is paused
        Assert.Multiple(async () =>
        {
            // Resume button should now be visible
            await WaitForElementAsync(ResumeButtonSelector);
            var resumeButton = Page.Locator(ResumeButtonSelector);
            Assert.That(await resumeButton.IsVisibleAsync(), Is.True, "Resume button should be visible when paused");

            // Cancel button should still be visible
            var cancelButton = Page.Locator(CancelButtonSelector);
            Assert.That(await cancelButton.IsVisibleAsync(), Is.True, "Cancel button should still be visible when paused");

            // Capture the time and verify it doesn't change
            var timeBeforeWait = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            await Task.Delay(2000);
            var timeAfterWait = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            
            Assert.That(timeAfterWait, Is.EqualTo(timeBeforeWait), 
                "Timer should not decrement when paused");
        });

        TestContext.Out.WriteLine("✓ Pomodoro paused successfully - timer stopped");
    }

    [Test]
    [Order(4)]
    [Description("UC-02: Resume a paused Pomodoro")]
    public async Task ResumePomodoro_ShouldReturnToRunningState()
    {
        // Arrange - Start and pause a Pomodoro
        await NavigateToAsync(BaseUrl);
        await ClickAndWaitAsync(StartButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);
        await ClickAndWaitAsync(PauseButtonSelector);
        await WaitForElementAsync(ResumeButtonSelector);

        // Act - Click Resume button
        await ClickAndWaitAsync(ResumeButtonSelector);

        // Assert - Verify timer is running again
        Assert.Multiple(async () =>
        {
            // Pause button should be visible again
            await WaitForElementAsync(PauseButtonSelector);
            var pauseButton = Page.Locator(PauseButtonSelector);
            Assert.That(await pauseButton.IsVisibleAsync(), Is.True, "Pause button should be visible after resume");

            // Timer should start counting down again
            var timeBeforeWait = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            await Task.Delay(1500);
            var timeAfterWait = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            
            Assert.That(timeAfterWait, Is.Not.EqualTo(timeBeforeWait), 
                "Timer should decrement after resume");
        });

        TestContext.Out.WriteLine("✓ Pomodoro resumed successfully - timer running again");
    }

    [Test]
    [Order(5)]
    [Description("UC-04: Complete a Pomodoro via confirmation modal")]
    public async Task CompletePomodoro_ShouldShowConfirmationAndReturnToReady()
    {
        // Arrange - Start a Pomodoro
        await NavigateToAsync(BaseUrl);
        await ClickAndWaitAsync(StartButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);

        // Act - Click Cancel to open confirmation modal
        await ClickAndWaitAsync(CancelButtonSelector);

        // Assert - Modal should appear
        await WaitForElementAsync(ModalBackdropSelector);
        var modal = Page.Locator(ModalBackdropSelector);
        Assert.That(await modal.IsVisibleAsync(), Is.True, "Confirmation modal should be visible");

        // Act - Click "Sí, marcar como completado"
        await ClickAndWaitAsync(ConfirmCompletedButtonSelector);

        // Assert - Should return to Ready state with Start button
        Assert.Multiple(async () =>
        {
            // Modal should close
            await Task.Delay(500);
            var modalAfter = Page.Locator(ModalBackdropSelector);
            Assert.That(await modalAfter.IsVisibleAsync(), Is.False, "Modal should close after confirmation");

            // Start button should be visible again
            await WaitForElementAsync(StartButtonSelector);
            var startButton = Page.Locator(StartButtonSelector);
            Assert.That(await startButton.IsVisibleAsync(), Is.True, "Start button should be visible after completion");

            // Timer should reset to 25:00
            var timerText = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            Assert.That(timerText, Is.EqualTo("25:00"), "Timer should reset to 25:00 after completion");
        });

        TestContext.Out.WriteLine("✓ Pomodoro completed successfully - returned to Ready state");
    }

    [Test]
    [Order(6)]
    [Description("UC-05: Start a short break after completing a Pomodoro")]
    public async Task StartShortBreak_ShouldChangeTimerToBreakMode()
    {
        // Arrange - Complete a Pomodoro first to enable break button
        await NavigateToAsync(BaseUrl);
        await ClickAndWaitAsync(StartButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);
        await ClickAndWaitAsync(CancelButtonSelector);
        await WaitForElementAsync(ModalBackdropSelector);
        await ClickAndWaitAsync(ConfirmCompletedButtonSelector);
        await WaitForElementAsync(StartButtonSelector);

        // Verify short break button is now visible
        await WaitForElementAsync(ShortBreakButtonSelector);

        // Act - Click Short Break button
        await ClickAndWaitAsync(ShortBreakButtonSelector);

        // Assert - Verify break mode is active
        Assert.Multiple(async () =>
        {
            // Pause button should be visible (for break)
            await WaitForElementAsync(PauseButtonSelector);
            var pauseButton = Page.Locator(PauseButtonSelector);
            Assert.That(await pauseButton.IsVisibleAsync(), Is.True, "Pause button should be visible during break");

            // Timer should show 5:00 (short break duration)
            var timerText = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            Assert.That(timerText, Is.EqualTo("05:00"), "Timer should show 5:00 for short break");

            // Timer should start counting down
            await Task.Delay(1500);
            var timeAfterDelay = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            Assert.That(timeAfterDelay, Is.Not.EqualTo("05:00"), 
                "Break timer should decrement when running");
        });

        TestContext.Out.WriteLine("✓ Short break started successfully - timer at 5:00");
    }

    [Test]
    [Order(7)]
    [Description("UC-06: Cancel a break without completing it")]
    public async Task CancelBreak_ShouldDiscardSessionAndReturnToReady()
    {
        // Arrange - Start a short break
        await NavigateToAsync(BaseUrl);
        await ClickAndWaitAsync(StartButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);
        await ClickAndWaitAsync(CancelButtonSelector);
        await WaitForElementAsync(ModalBackdropSelector);
        await ClickAndWaitAsync(ConfirmCompletedButtonSelector);
        await WaitForElementAsync(StartButtonSelector);
        await ClickAndWaitAsync(ShortBreakButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);

        // Act - Click Cancel to open confirmation modal
        await ClickAndWaitAsync(CancelButtonSelector);

        // Assert - Modal should appear
        await WaitForElementAsync(ModalBackdropSelector);
        var modal = Page.Locator(ModalBackdropSelector);
        Assert.That(await modal.IsVisibleAsync(), Is.True, "Confirmation modal should be visible");

        // Act - Click "No, descartar sesión"
        await ClickAndWaitAsync(ConfirmNotCompletedButtonSelector);

        // Assert - Should return to Ready state
        Assert.Multiple(async () =>
        {
            // Modal should close
            await Task.Delay(500);
            var modalAfter = Page.Locator(ModalBackdropSelector);
            Assert.That(await modalAfter.IsVisibleAsync(), Is.False, "Modal should close after cancellation");

            // Start button should be visible
            await WaitForElementAsync(StartButtonSelector);
            var startButton = Page.Locator(StartButtonSelector);
            Assert.That(await startButton.IsVisibleAsync(), Is.True, "Start button should be visible after cancel");

            // Timer should reset to 25:00 (Pomodoro duration) when returning to Ready
            var timerText = await Page.Locator(TimerDisplaySelector).TextContentAsync();
            Assert.That(timerText, Is.EqualTo("25:00"), "Timer should reset to 25:00 after break cancellation");
        });

        TestContext.Out.WriteLine("✓ Break cancelled successfully - discarded and returned to Ready (timer at 25:00)");
    }

    [Test]
    [Order(8)]
    [Description("Full Happy Path: Complete workflow from start to finish")]
    public async Task FullHappyPath_ShouldCompleteAllStepsSuccessfully()
    {
        TestContext.Out.WriteLine("=== Starting Full Happy Path Test ===");

        // Step 1: Load page
        TestContext.Out.WriteLine("Step 1: Loading page...");
        await NavigateToAsync(BaseUrl);
        await WaitForElementAsync(TimerDisplaySelector);
        Assert.That(await Page.Locator(TimerDisplaySelector).TextContentAsync(), Is.EqualTo("25:00"));

        // Step 2: Start Pomodoro
        TestContext.Out.WriteLine("Step 2: Starting Pomodoro...");
        await ClickAndWaitAsync(StartButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);
        Assert.That(await Page.Locator(PauseButtonSelector).IsVisibleAsync(), Is.True);

        // Step 3: Pause
        TestContext.Out.WriteLine("Step 3: Pausing Pomodoro...");
        await ClickAndWaitAsync(PauseButtonSelector);
        await WaitForElementAsync(ResumeButtonSelector);
        Assert.That(await Page.Locator(ResumeButtonSelector).IsVisibleAsync(), Is.True);

        // Step 4: Resume
        TestContext.Out.WriteLine("Step 4: Resuming Pomodoro...");
        await ClickAndWaitAsync(ResumeButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);
        Assert.That(await Page.Locator(PauseButtonSelector).IsVisibleAsync(), Is.True);

        // Step 5: Complete (via modal)
        TestContext.Out.WriteLine("Step 5: Completing Pomodoro...");
        await ClickAndWaitAsync(CancelButtonSelector);
        await WaitForElementAsync(ModalBackdropSelector);
        await ClickAndWaitAsync(ConfirmCompletedButtonSelector);
        await WaitForElementAsync(StartButtonSelector);
        Assert.That(await Page.Locator(StartButtonSelector).IsVisibleAsync(), Is.True);

        // Step 6: Start short break
        TestContext.Out.WriteLine("Step 6: Starting short break...");
        await WaitForElementAsync(ShortBreakButtonSelector);
        await ClickAndWaitAsync(ShortBreakButtonSelector);
        await WaitForElementAsync(PauseButtonSelector);
        var breakTime = await Page.Locator(TimerDisplaySelector).TextContentAsync();
        Assert.That(breakTime, Is.EqualTo("05:00"));

        // Step 7: Cancel break
        TestContext.Out.WriteLine("Step 7: Cancelling break...");
        await ClickAndWaitAsync(CancelButtonSelector);
        await WaitForElementAsync(ModalBackdropSelector);
        await ClickAndWaitAsync(ConfirmNotCompletedButtonSelector);
        await WaitForElementAsync(StartButtonSelector);

        // Final verification
        TestContext.Out.WriteLine("Final verification...");
        var finalTimerText = await Page.Locator(TimerDisplaySelector).TextContentAsync();
        Assert.That(finalTimerText, Is.EqualTo("25:00"), "Timer should reset to 25:00 after break cancellation");
        Assert.That(await HasBlazorErrorAsync(), Is.False, "Should have no Blazor errors");

        TestContext.Out.WriteLine("=== Full Happy Path Completed Successfully ===");
        
        // Capture final screenshot
        try
        {
            var screenshotPath = await CaptureScreenshotAsync("HappyPath_Complete");
            TestContext.Out.WriteLine($"Screenshot saved: {screenshotPath}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Screenshot capture failed: {ex.Message}");
        }
    }
}
