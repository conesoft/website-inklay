using Conesoft.Hosting;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Options;
using Serilog;

namespace Conesoft.Website.Inklay.Services;

public class CalendarCache(IHttpClientFactory factory, IOptions<CalendarCache.Settings> settings, TimeSpan period) : PeriodicCache<IGrouping<DateTime, CalendarCache.Entry>[]>(period)
{
    private HttpClient Client => factory.CreateClient();

    protected override async Task<IGrouping<DateTime, Entry>[]> Generate()
    {
        Log.Information("updating calendar cache");
        try
        {
            var all = await Task.WhenAll(settings.Value.Calendars.Select(c => GetCalendar(c.Key, c.Value)));
            return [.. all
                .SelectMany(e => e)
                .GroupBy(e => e.Occurrence.Period.StartTime.Date)];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "failed to update calendar data");
            return [];
        }
    }

    private async Task<Entry[]> GetCalendar(string name, string source)
    {
        try
        {
            var contents = await Client.GetStringAsync(source);
            var calendar = Ical.Net.Calendar.Load(contents);

            return [.. calendar
                           .GetOccurrences(DateTime.Today, DateTime.Today.AddDays(10))
                           .Where(o => o.Source is CalendarEvent && o.Period != null)
                           .Select(o => new Entry(name, o))];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "failed to update calendar data");
            return [];
        }
    }

    public record Entry(string Name, Occurrence Occurrence);
    public record Settings(Dictionary<string, string> Calendars)
    {
        public Settings() : this([])
        {
        }
    };
}
