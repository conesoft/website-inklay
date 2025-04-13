using Conesoft.Hosting;
using Conesoft.PwaGenerator;
using Conesoft.Website.Inklay.Components;
using Conesoft.Website.Inklay.Services;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddHostConfigurationFiles(configurator =>
    {
        configurator.Add<CalendarCache.Settings>();
    })
    .AddHostEnvironmentInfo()
    .AddLoggingService()
    .AddUsersWithStorage()
    .AddCompiledHashCacheBuster()
    .AddHostingDefaults()
    ;

builder.Services
    .AddHostedServiceWith<CalendarCache>(TimeSpan.FromMinutes(5))
    .AddHostedServiceWith<WeatherCache>(TimeSpan.FromMinutes(30))
    ;

var app = builder.Build();

app.UseCompiledHashCacheBuster();
app.MapPwaInformationFromAppSettings();
app.MapUsersWithStorage();
app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();

