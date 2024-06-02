using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Conesoft.Website.Inklay.Services
{
    public class CalendarCache(IHttpClientFactory factory, TimeSpan period) : Cache<IGrouping<DateTime, Occurrence>[]>(factory, period)
    {
        protected override async Task<IGrouping<DateTime, Occurrence>[]> Generate()
        {
            if ((await Hosting.Host.LocalSettings.ReadFromJson<Settings>()) is Settings settings)
            {
                var contents = await Client.GetStringAsync(settings.Calendar);
                var calendar = Ical.Net.Calendar.Load(contents);
                return calendar
                               .GetOccurrences(DateTime.Today, DateTime.Today.AddDays(7))
                               .Where(o => o.Source is CalendarEvent)
                               .OrderBy(o => o.Period)
                               .GroupBy(o => o.Period.StartTime.Date)
                               .ToArray();
            }
            return [];
        }

        record Settings(string Calendar);
    }
}
