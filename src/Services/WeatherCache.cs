using OpenMeteo;

namespace Conesoft.Website.Inklay.Services;

public class WeatherCache(IHttpClientFactory factory, TimeSpan period) : Cache<WeatherForecast>(factory, period)
{
    private readonly OpenMeteoClient client = new();
    private readonly WeatherForecastOptions options = new() { Current = CurrentOptions.All, Daily = DailyOptions.All };

    protected override async Task<WeatherForecast> Generate() => (await client.QueryAsync("Menziken", options))!;
}
