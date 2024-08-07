using Prometheus;
using Scraper.Services;

namespace Scraper;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });

        builder.Services.AddControllers();
        builder.AddScraper();

        var app = builder.Build();

        app.UseMetricServer();

        app.Run();
    }
}
