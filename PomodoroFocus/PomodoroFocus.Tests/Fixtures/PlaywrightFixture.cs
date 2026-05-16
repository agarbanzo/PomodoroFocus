using Microsoft.Playwright;

namespace PomodoroFocus.Tests.Fixtures;

/// <summary>
/// Fixture for managing Playwright browser context across E2E tests.
/// Follows Blazor testing best practices: waits for DOM elements, uses data-test selectors.
/// </summary>
[SetUpFixture]
public class PlaywrightFixture
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;

    protected IPage Page => _context?.Pages.FirstOrDefault() 
        ?? throw new InvalidOperationException("Browser context not initialized. Call InitializeAsync first.");

    [OneTimeSetUp]
    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        
        // Launch browser in headed mode for debugging, or headless for CI
        var headed = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADED") == "true";
        
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = !headed,
            SlowMo = 100, // Slow down for Blazor rendering
            Args = new[] { "--start-maximized" }
        });

        // Create context with Blazor-friendly viewport
        _context = await _browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true,
            BaseURL = "http://localhost:5294"
        });

        // Create initial page
        await _context.NewPageAsync();
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if (_context != null)
            await _context.CloseAsync();
        
        if (_browser != null)
            await _browser.CloseAsync();
        
        _playwright?.Dispose();
    }

    /// <summary>
    /// Navigate to a page and wait for Blazor to render.
    /// </summary>
    public async Task NavigateToAsync(string url)
    {
        await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.Load, Timeout = 30000 });
        // Wait for Blazor to be ready - increased timeout for WASM startup
        await Page.WaitForSelectorAsync(".timer-display-root", new() { Timeout = 30000, State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Wait for a specific element to be visible (Blazor render strategy).
    /// </summary>
    public async Task WaitForElementAsync(string selector, int timeoutMs = 5000)
    {
        await Page.WaitForSelectorAsync(selector, new() { Timeout = timeoutMs, State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Wait for an element to contain specific text.
    /// </summary>
    public async Task WaitForTextAsync(string selector, string text, int timeoutMs = 5000)
    {
        await Page.WaitForSelectorAsync($"{selector}:has-text(\"{text}\")", new() { Timeout = timeoutMs });
    }

    /// <summary>
    /// Click an element and wait for Blazor to process.
    /// </summary>
    public async Task ClickAndWaitAsync(string selector, int delayMs = 300)
    {
        await Page.ClickAsync(selector);
        await Task.Delay(delayMs); // Allow Blazor to re-render
    }

    /// <summary>
    /// Check if Blazor error UI is visible (unhandled exceptions).
    /// </summary>
    public async Task<bool> HasBlazorErrorAsync()
    {
        var errorSelector = "#blazor-error-ui";
        try
        {
            var errorElement = Page.Locator(errorSelector);
            return await errorElement.IsVisibleAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Capture a screenshot for debugging.
    /// </summary>
    public async Task<string> CaptureScreenshotAsync(string testName)
    {
        var screenshotPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "screenshots", $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
        await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
        return screenshotPath;
    }
}
