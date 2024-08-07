namespace Scraper.Services;

/// <summary>
/// Base exception for all exceptions thrown by the scraper.
/// </summary>
public class ScraperException : Exception
{
    public ScraperException(string message)
        : base(message)
    {
    }

    public ScraperException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
