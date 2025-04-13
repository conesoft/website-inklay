using Conesoft.Hosting;
using OpenMeteo;
using Serilog;

namespace Conesoft.Website.Inklay.Services;

public class WeatherCache(TimeSpan period) : PeriodicCache<CachedWeatherData>(period)
{
    private readonly OpenMeteoClient client = new();
    private readonly WeatherForecastOptions options = new() { Current = CurrentOptions.All, Daily = DailyOptions.All, Timezone = "auto" };

    protected override async Task<CachedWeatherData> Generate()
    {
        Log.Information("updating weather cache");
        var weatherForecast = (await client.QueryAsync("Menziken", options))!;

        var airQuality = await client.QueryAsync(new AirQualityOptions(weatherForecast.Latitude, weatherForecast.Longitude, AirQualityOptions.HourlyOptions.All, "auto", "iso8601", "auto", 0, DateTime.Today.ToString("O"), DateTime.Today.ToString("O")));

        Dictionary<DailyAirSamples.PollenType, float[]> pollen = [];

        pollen[DailyAirSamples.PollenType.Alder] = (airQuality?.Hourly?.Alder_pollen?.Where(p => p != null).Cast<float>().ToArray())!;
        pollen[DailyAirSamples.PollenType.Birch] = (airQuality?.Hourly?.Birch_pollen?.Where(p => p != null).Cast<float>().ToArray())!;
        pollen[DailyAirSamples.PollenType.Grass] = (airQuality?.Hourly?.Grass_pollen?.Where(p => p != null).Cast<float>().ToArray())!;
        pollen[DailyAirSamples.PollenType.Mugwort] = (airQuality?.Hourly?.Mugwort_pollen?.Where(p => p != null).Cast<float>().ToArray())!;
        pollen[DailyAirSamples.PollenType.Ragweed] = (airQuality?.Hourly?.Ragweed_pollen?.Where(p => p != null).Cast<float>().ToArray())!;

        var daily = GroupNextWeek(pollen);

        var unit = airQuality?.Hourly_Units?.Birch_pollen ?? "";

        return new CachedWeatherData(weatherForecast, [.. daily], unit);
    }


    static IEnumerable<DailyAirSamples> GroupNextWeek(Dictionary<DailyAirSamples.PollenType, float[]> data)
    {
        for (var i = 0; i < 7; i++)
        {
            yield return new DailyAirSamples(DateTime.Today.AddDays(i), data.ToDictionary(d => d.Key, d => d.Value.Skip(i * 24).Take(24).ToArray()));
        }
    }
}

public record DailyAirSamples(DateTime Day, Dictionary<DailyAirSamples.PollenType, float[]> Samples)
{
    // https://www.meteoswiss.admin.ch/dam/jcr:43b4f361-8bc1-4af7-a232-c126de0f2f80/Belastungsklassen-der-allergenen-Pollenarten_E.pdf

    record Thresholds(float Medium, float High);
    enum Threshold { Low, Medium, High }
    public enum PollenType { Alder, Birch, Grass, Mugwort, Ragweed }

    readonly Dictionary<PollenType, Thresholds> thresholds = new()
    {
        [PollenType.Alder] = new(70, 250),
        [PollenType.Birch] = new(70, 350),
        [PollenType.Grass] = new(50, 150),
        [PollenType.Mugwort] = new(15, 50),
        [PollenType.Ragweed] = new(10, 30)
    };

    public string GetAirQualityGraph()
    {
        var stats = Enumerable
            .Range(0, 4)
            .Select(i => Samples.Aggregate(default(Threshold?), (accumulation, s) =>
            {
                var pollenType = s.Key;
                var samples = s.Value.Skip(i * 6).Take(6);

                var high = thresholds[pollenType].High;
                var medium = thresholds[pollenType].Medium;

                return samples.Aggregate(accumulation, (acc, value) =>
                {
                    if (value >= high || acc == Threshold.High) return Threshold.High;
                    if (value >= medium || acc == Threshold.Medium) return Threshold.Medium;
                    return Threshold.Low;
                });
            }))
            .ToArray();

        // ◌○●
        return stats.Length > 0 ? string.Join("", stats.Select(stat => stat switch
        {
            Threshold.High => '●',
            Threshold.Medium => '○',
            Threshold.Low =>  '◌',
            _ => ' '
        })) : " ";
    }
};

public record CachedWeatherData(WeatherForecast WeatherForecast, DailyAirSamples[] DailyAirSamples, string DailyAirSampleUnit);