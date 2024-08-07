using Flurl.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scraper.Models;
using Scraper.Services;

namespace Scraper;

internal static class HostExtensions
{
    public static IHostApplicationBuilder AddScraper(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton(TimeProvider.System);

        builder.Services.Configure<Tc4400ConfigModel>(builder.Configuration.GetSection("Modem:Tc4400"));

        builder.Services.AddKeyedSingleton(ScraperBackgroundService.DefaultIntervalKey, new Ref<TimeSpan>(ScraperBackgroundService.DefaultInterval));
        builder.Services.AddSingleton<ScraperBackgroundService>();
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<ScraperBackgroundService>());
        builder.Services.AddSingleton<ScraperOperation>();
        builder.Services.AddKeyedSingleton<IFlurlClient>(Tc4400Client.ModemClientKey, new FlurlClient());
        builder.Services.AddSingleton<Tc4400Client>();

        return builder;
    }
}
