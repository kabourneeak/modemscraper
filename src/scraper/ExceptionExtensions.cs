namespace Scraper;

internal static class ExceptionExtensions
{
    /// <summary>
    /// Gets <see cref="Exception.Message" of <paramref name="ex"/> and its inner exceptions.
    /// </summary>
    /// <param name="ex">The exception to get message for.</param>
    /// <returns>A single string of all exception messages.</returns>
    public static string GetAllMessages(this Exception ex)
    {
        return string.Join(". ", ex.GetMessages());
    }

    private static IEnumerable<string> GetMessages(this Exception ex)
    {
        var current = ex;
        while (current is not null)
        {
            yield return current.Message;
            current = current.InnerException;
        }
    }
}