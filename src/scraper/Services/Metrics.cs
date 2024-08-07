using Prometheus;

namespace Scraper.Services;

internal static class Metrics
{
    public static readonly Counter ScrapeCount = Prometheus.Metrics.CreateCounter(
        "scraper_scrape_count",
        "Number of scrapes performed by the scraper service."
    );

    public static readonly Gauge ScrapeOperationState = Prometheus.Metrics.CreateGauge(
        "scraper_scrape_operation_state",
        "The health of different aspects of scrape operations.",
        new GaugeConfiguration
        {
            LabelNames = ["operation", "state"]
        });

    public static readonly Gauge UpstreamChannelTxLevel = Prometheus.Metrics.CreateGauge(
        "scraper_upstream_channel_tx_level",
        "Upstream channel transmit level.",
        new GaugeConfiguration
        {
            LabelNames = ["channel_index"]
        }
    );

    public static readonly Gauge UpstreamChannelBondingState = Prometheus.Metrics.CreateGauge(
        "scraper_upstream_channel_bonding_state",
        "Upstream channel bonding state.",
        new GaugeConfiguration
        {
            LabelNames = ["channel_index", "state"]
        }
    );

    public static readonly Gauge DownstreamChannelRxLevel = Prometheus.Metrics.CreateGauge(
        "scraper_downstream_channel_rx_level",
        "Downstream channel receive level.",
        new GaugeConfiguration
        {
            LabelNames = ["channel_index"]
        }
    );

    public static readonly Gauge DownstreamChannelSnrLevel = Prometheus.Metrics.CreateGauge(
        "scraper_downstream_channel_snr_level",
        "Downstream channel signal-to-noise ratio level.",
        new GaugeConfiguration
        {
            LabelNames = ["channel_index"]
        }
    );

    public static readonly Gauge DownstreamChannelBondingState = Prometheus.Metrics.CreateGauge(
        "scraper_downstream_channel_bonding_state",
        "Downstream channel bonding state.",
        new GaugeConfiguration
        {
            LabelNames = ["channel_index", "state"]
        }
    );
}