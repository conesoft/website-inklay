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
    record Thresholds(float Medium, float High, float VeryHigh);
    public enum Threshold { NoValue, Low, Medium, High, VeryHigh, MaxValue }
    public enum PollenType { Alder, Birch, Grass, Mugwort, Ragweed }

    // https://www.meteoswiss.admin.ch/dam/jcr:43b4f361-8bc1-4af7-a232-c126de0f2f80/Belastungsklassen-der-allergenen-Pollenarten_E.pdf
    readonly Dictionary<PollenType, Thresholds> thresholds = new()
    {
        [PollenType.Alder] = new(10, 70, 250),
        [PollenType.Birch] = new(10, 70, 350),
        [PollenType.Grass] = new(20, 50, 150),
        [PollenType.Mugwort] = new(5, 15, 50),
        [PollenType.Ragweed] = new(5, 10, 30)
    };

    public Threshold[] GetPeaks() => [.. Enumerable
        .Range(0, 24)
        .Select(i => Samples.Aggregate(Threshold.NoValue, (acc, s) =>
        {
            var pollenType = s.Key;
            var sample = i < s.Value.Length ? s.Value[i] : default(float?);

            var veryhigh = thresholds[pollenType].VeryHigh;
            var high = thresholds[pollenType].High;
            var medium = thresholds[pollenType].Medium;

            if(sample == null) return acc;

            if (sample >= veryhigh || acc == Threshold.VeryHigh) return Threshold.VeryHigh;
            if (sample >= high || acc == Threshold.High) return Threshold.High;
            if (sample >= medium || acc == Threshold.Medium) return Threshold.Medium;
            return Threshold.Low;
        }))
    ];


    public Dictionary<Threshold, List<Range>> GetPeakRanges()
    {
        var peaks = GetPeaks();
        var ranges = new Dictionary<Threshold, List<Range>>();
        for (var threshold = Threshold.NoValue; threshold < Threshold.MaxValue; threshold++)
        {
            if(ranges.ContainsKey(threshold) == false)
            {
                ranges[threshold] = [];
            }
            var start = 0;
            var end = 0;
            for (var i = 1; i < peaks.Length; i++)
            {
                if (peaks[i - 1] < threshold && peaks[i] >= threshold)
                {
                    start = i;
                }
                if (peaks[i - 1] >= threshold && peaks[i] < threshold)
                {
                    end = i;
                    ranges[threshold].Add(new Range(start, end));
                }
            }
        }
        return ranges;
    }
};

public record CachedWeatherData(WeatherForecast WeatherForecast, DailyAirSamples[] DailyAirSamples, string DailyAirSampleUnit);