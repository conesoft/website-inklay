using Conesoft.Files;
using Conesoft.Website.Inklay.Components;
using Conesoft.Website.Inklay.Services;
using Conesoft.Website.Inklay.Tools;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var log = Conesoft.Hosting.Host.Root / Filename.From($"{Conesoft.Hosting.Host.Name.ToLowerInvariant()} log", "txt");

System.IO.File.WriteAllText(log.Path, "");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(log.Path, buffered: false, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();


Log.Information("starting app");

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddHostedServiceWith<GarbageCollect>(TimeSpan.FromMinutes(1));
builder.Services.AddHostedServiceWith<CalendarCache>(TimeSpan.FromMinutes(5));
builder.Services.AddHostedServiceWith<WeatherCache>(TimeSpan.FromMinutes(30));
builder.Services.AddRazorComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app
    .UseDeveloperExceptionPage()
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();

