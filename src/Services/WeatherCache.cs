using Conesoft.Hosting;
using OpenMeteo;
using Serilog;

namespace Conesoft.Website.Inklay.Services;

public class WeatherCache(TimeSpan period) : PeriodicCache<WeatherForecast>(period)
{
    private readonly OpenMeteoClient client = new();
    private readonly WeatherForecastOptions options = new() { Current = CurrentOptions.All, Daily = DailyOptions.All };

    protected override async Task<WeatherForecast> Generate()
    {
        Log.Information("updating weather cache");
        return (await client.QueryAsync("Menziken", options))!;
    }
}
