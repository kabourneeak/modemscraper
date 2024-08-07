using System.Diagnostics;

namespace Scraper.Services;

internal class ScraperBackgroundService : BackgroundService
{
    internal const string DefaultIntervalKey = "Scraper:Interval";
    internal static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(30);

    private readonly TimeProvider timeProvider;
    private readonly ScraperOperation scraperOperation;
    private readonly Ref<TimeSpan> interval;
    private readonly ILogger<ScraperBackgroundService> logger;

    public ScraperBackgroundService(
        TimeProvider timeProvider,
        ScraperOperation scraperOperation,
        [FromKeyedServices(DefaultIntervalKey)] Ref<TimeSpan> interval,
        ILogger<ScraperBackgroundService> logger)
    {
        this.timeProvider = timeProvider;
        this.scraperOperation = scraperOperation;
        this.interval = interval;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await scraperOperation.InvokeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical("Unhandled exception occurred while scraping. {Messages}", ex.GetAllMessages());
                logger.LogDebug(ex, "Unhandled exception occurred while scraping.");
            }

            // figure out how long we need to wait
            // to make up our desired interval
            stopwatch.Stop();

            var elapsed = stopwatch.Elapsed;

            // consider the case where the scrape took longer than the interval
            var nextDelay = elapsed < interval.Value
                ? interval.Value - elapsed
                : TimeSpan.FromMilliseconds(1); // some small non-zero delay

            logger.LogInformation("Next scrape in {nextDelay:F1} seconds.", nextDelay.TotalSeconds);

            await Task.Delay(nextDelay, timeProvider, stoppingToken);
        }
    }
}
