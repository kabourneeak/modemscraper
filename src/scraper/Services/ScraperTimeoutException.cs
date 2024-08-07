namespace Scraper.Services;

public class ScraperTimeoutException : ScraperException
{
    public ScraperTimeoutException(string message)
        : base(message)
    {
    }

    public ScraperTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}