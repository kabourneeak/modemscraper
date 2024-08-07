namespace Scraper.Services;

/// <summary>
/// Represents the bonding state of a channel.
/// </summary>
internal enum BondingState
{
    NotBonded,

    /// <summary>
    /// Appears as "Partial Service" in the Modem's web interface.
    /// </summary>
    Partial,

    /// <summary>
    /// Appears as "Bonded" in the Modem's web interface.
    /// </summary>
    Bonded,
}
