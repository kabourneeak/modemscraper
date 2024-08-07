namespace Scraper.Services;

internal enum OperationState
{
    /// <summary>
    /// The state of the operation is unknown or was not able to be evaluated.
    /// </summary>
    Unknown,

    /// <summary>
    /// The operation is not entirely healthy.
    /// </summary>
    Degraded,

    /// <summary>
    /// The operation has failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The operation is healthy.
    /// </summary>
    Healthy,
}