using Conesoft.Hosting;
using Conesoft.Website.Inklay.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddHostConfigurationFiles(configurator =>
{
    configurator.Add<CalendarCache.Settings>();
});
builder.AddHostEnvironmentInfo();
builder.AddLoggingService();

// Add services to the container.
builder.Services
    .AddCompiledHashCacheBuster()
    .AddHttpClient()
    .AddHostedServiceWith<CalendarCache>(TimeSpan.FromMinutes(5))
    .AddHostedServiceWith<WeatherCache>(TimeSpan.FromMinutes(30))
    .AddRazorComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseLoggingServiceOnRequests();

app
    .UseCompiledHashCacheBuster()
    .UseDeveloperExceptionPage()
    .UseHttpsRedirection()
    .UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<Conesoft.Website.Inklay.Components.App>();

app.Run();

