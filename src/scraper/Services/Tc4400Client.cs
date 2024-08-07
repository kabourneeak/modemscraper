using Flurl.Http;
using Microsoft.Extensions.Options;
using Scraper.Models;

namespace Scraper.Services;

internal class Tc4400Client
{
    internal const string ModemClientKey = "ModemClient";
    private readonly IOptionsMonitor<Tc4400ConfigModel> options;
    private readonly IFlurlClient flurlClient;

    public Tc4400Client(
        IOptionsMonitor<Tc4400ConfigModel> options,
        [FromKeyedServices(ModemClientKey)] IFlurlClient flurlClient
    )
    {
        this.options = options;
        this.flurlClient = flurlClient;
    }

    public async Task<string> GetCmConnectionStatus(CancellationToken cancellationToken)
    {
        var host = options.CurrentValue.Host ?? "http://192.168.100.1";
        var username = options.CurrentValue.Username ?? "admin";
        var password = options.CurrentValue.Password
            ?? throw new ScraperException("TC4440 Modem Password needs to be configured.");

        try
        {
            var response = await flurlClient
                .Request(host, "cmconnectionstatus.html")
                .WithBasicAuth(username, password)
                .WithTimeout(TimeSpan.FromSeconds(30))
                .GetStringAsync(cancellationToken: cancellationToken);

            return response;
        }
        catch (FlurlHttpTimeoutException ex)
        {
            throw new ScraperTimeoutException("Timeout occurred while fetching CM connection status.", ex);
        }
        catch (FlurlHttpException ex)
        {
            throw new ScraperException("Error occurred while fetching CM connection status.", ex);
        }
    }
}
