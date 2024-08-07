using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Scraper.Services;

internal class ScraperOperation
{
    private const string ModemReachableOp = "modem_reachable";
    private const string HtmlParsableOp = "html_parsable";
    private const string UpstreamParseableOp = "upstream_parseable";
    private const string DownstreamParseableOp = "downstream_parseable";

    private readonly Tc4400Client client;
    private readonly ILogger<ScraperOperation> logger;

    public ScraperOperation(
        Tc4400Client client,
        ILogger<ScraperOperation> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("Scraping...");

        Metrics.ScrapeCount.Inc();

        var scrapeOperationState = await ScrapeAsync(cancellationToken);

        SetOperationState(ModemReachableOp, scrapeOperationState.ModemReachable);
        SetOperationState(HtmlParsableOp, scrapeOperationState.HtmlParsable);
        SetOperationState(UpstreamParseableOp, scrapeOperationState.UpstreamParseable);
        SetOperationState(DownstreamParseableOp, scrapeOperationState.DownstreamParseable);
    }

    private async Task<ScrapeOperationState> ScrapeAsync(CancellationToken cancellationToken)
    {
        var scrapeOperationState = new ScrapeOperationState();
        string connectionStatusRaw;

        try
        {
            connectionStatusRaw = await client.GetCmConnectionStatus(cancellationToken);

            scrapeOperationState = scrapeOperationState with { ModemReachable = OperationState.Healthy };
        }
        catch (ScraperException ex)
        {
            logger.LogWarning("Failed to fetch CM connection status. {Messages}", ex.GetAllMessages());
            logger.LogDebug(ex, "Failed to fetch CM connection status");

            scrapeOperationState = scrapeOperationState with { ModemReachable = OperationState.Failed };

            return scrapeOperationState;
        }

        // keep a copy just for fun
        await File.WriteAllTextAsync("last-cmconnectionstatus.html", connectionStatusRaw, cancellationToken);

        // parse the HTML
        var htmlParser = new HtmlParser();
        IHtmlDocument htmlDoc;

        try
        {
            htmlDoc = htmlParser.ParseDocument(connectionStatusRaw);
            scrapeOperationState = scrapeOperationState with { HtmlParsable = OperationState.Healthy };
        }
        catch (Exception ex)
        {
            logger.LogWarning("Failed to parse HTML document. {Messages}", ex.GetAllMessages());
            logger.LogDebug(ex, "Failed to parse HTML document");

            scrapeOperationState = scrapeOperationState with { HtmlParsable = OperationState.Failed };

            return scrapeOperationState;
        }

        using (htmlDoc)
        {
            try
            {
                ParseUpstreamChannels(htmlDoc);
                scrapeOperationState = scrapeOperationState with { UpstreamParseable = OperationState.Healthy };
            }
            catch (Exception ex)
            {
                logger.LogWarning("Failed to parse upstream channels. {Messages}", ex.GetAllMessages());
                logger.LogDebug(ex, "Failed to parse upstream channels");
                scrapeOperationState = scrapeOperationState with { UpstreamParseable = OperationState.Failed };
            }

            try
            {
                ParseDownstreamChannels(htmlDoc);
                scrapeOperationState = scrapeOperationState with { DownstreamParseable = OperationState.Healthy };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse downstream channels. {Messages}", ex.GetAllMessages());
                logger.LogDebug(ex, "Failed to parse downstream channels");

                scrapeOperationState = scrapeOperationState with { DownstreamParseable = OperationState.Failed };
            }
        }

        return scrapeOperationState;
    }

    private void ParseUpstreamChannels(IHtmlDocument htmlDoc)
    {
        // select Upstream Channel Status table
        // which is the 3rd table
        var upstreamTable = htmlDoc.QuerySelectorAll("table")[2];

        // select all rows except the title and the column names
        var rows = upstreamTable.QuerySelectorAll("tr").Skip(2);

        foreach (var row in rows)
        {
            var cells = row.QuerySelectorAll("td");

            var channelIndex = int.Parse(cells[0].TextContent);
            var bondingState = ParseBondingState(cells[4].TextContent);
            var txLevel = ParseDbmvLevelOrDefault(cells[7].TextContent) ?? 0.0;

            logger.LogInformation(
                "Upstream Channel {ChannelIndex}: {BondingState}, Tx Level: {TxLevel:F1} dBmV",
                channelIndex,
                bondingState,
                txLevel);

            Metrics.UpstreamChannelTxLevel
                .WithLabels(channelIndex.ToString())
                .Set(txLevel);

            Metrics.UpstreamChannelBondingState
                .WithLabels(channelIndex.ToString(), BondingState.Bonded.ToString())
                .Set(bondingState == BondingState.Bonded ? 1 : 0);

            Metrics.UpstreamChannelBondingState
                .WithLabels(channelIndex.ToString(), BondingState.Partial.ToString())
                .Set(bondingState == BondingState.Partial ? 1 : 0);

            Metrics.UpstreamChannelBondingState
                .WithLabels(channelIndex.ToString(), BondingState.NotBonded.ToString())
                .Set(bondingState == BondingState.NotBonded ? 1 : 0);
        }
    }

    private void ParseDownstreamChannels(IHtmlDocument htmlDoc)
    {
        // select Downstream Channel Status table
        // which is the 2nd table
        var downstreamTable = htmlDoc.QuerySelectorAll("table")[1];

        // select all rows except the title and the column names
        var rows = downstreamTable.QuerySelectorAll("tr").Skip(2);

        foreach (var row in rows)
        {
            var cells = row.QuerySelectorAll("td");

            var channelIndex = int.Parse(cells[0].TextContent);
            var bondingState = ParseBondingState(cells[4].TextContent);
            var snrLevel = ParseDbLevelOrDefault(cells[7].TextContent) ?? 0.0;
            var rxLevel = ParseDbmvLevelOrDefault(cells[8].TextContent) ?? 0.0;

            logger.LogInformation(
                "Downstream Channel {ChannelIndex}: {BondingState}, Snr Level: {SnrLevel:F1} dB, Rx Level: {RxLevel:F1} dBmV",
                channelIndex,
                bondingState,
                snrLevel,
                rxLevel);

            Metrics.DownstreamChannelRxLevel
                .WithLabels(channelIndex.ToString())
                .Set(rxLevel);

            Metrics.DownstreamChannelSnrLevel
                .WithLabels(channelIndex.ToString())
                .Set(snrLevel);

            Metrics.DownstreamChannelBondingState
                .WithLabels(channelIndex.ToString(), BondingState.Bonded.ToString())
                .Set(bondingState == BondingState.Bonded ? 1 : 0);

            Metrics.DownstreamChannelBondingState
                .WithLabels(channelIndex.ToString(), BondingState.Partial.ToString())
                .Set(bondingState == BondingState.Partial ? 1 : 0);

            Metrics.DownstreamChannelBondingState
                .WithLabels(channelIndex.ToString(), BondingState.NotBonded.ToString())
                .Set(bondingState == BondingState.NotBonded ? 1 : 0);
        }
    }

    private static BondingState ParseBondingState(string bondingState)
    {
        return bondingState switch
        {
            "Bonded" => BondingState.Bonded,
            "Partial Service" => BondingState.Partial,
            _ => BondingState.NotBonded
        };
    }

    private static double? ParseDbmvLevelOrDefault(string txLevel) => ParseDoubleOrDefault(txLevel, " dBmV");

    private static double? ParseDbLevelOrDefault(string txLevel) => ParseDoubleOrDefault(txLevel, " dB");

    private static double? ParseDoubleOrDefault(string value, string unit)
    {
        if (double.TryParse(value.Replace(unit, string.Empty), out var result))
        {
            return result;
        }

        return null;
    }

    private static void SetOperationState(string operation, OperationState state)
    {
        Metrics.ScrapeOperationState
            .WithLabels(operation, OperationState.Unknown.ToString())
            .Set(state == OperationState.Unknown ? 1 : 0);

        Metrics.ScrapeOperationState
            .WithLabels(operation, OperationState.Failed.ToString())
            .Set(state == OperationState.Failed ? 1 : 0);

        Metrics.ScrapeOperationState
            .WithLabels(operation, OperationState.Degraded.ToString())
            .Set(state == OperationState.Degraded ? 1 : 0);

        Metrics.ScrapeOperationState
            .WithLabels(operation, OperationState.Healthy.ToString())
            .Set(state == OperationState.Healthy ? 1 : 0);
    }

    private sealed record ScrapeOperationState
    {
        public OperationState ModemReachable { get; init; }

        public OperationState HtmlParsable { get; init; }

        public OperationState UpstreamParseable { get; init; }

        public OperationState DownstreamParseable { get; init; }
    }
}