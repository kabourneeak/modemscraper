namespace Scraper.Models;

internal record Tc4400ConfigModel
{
    /// <summary>
    /// The host address of the TC4400 modem. Defaults to <c>http://192.168.100.1</c>.
    /// </summary>
    public string? Host { get; init; }

    /// <summary>
    /// The username to use when authenticating with the TC4400 modem. Defaults to <c>admin</c>.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// The password to use when authenticating with the TC4400 modem. Must be configured.
    /// </summary>
    public string? Password { get; init; }
}
