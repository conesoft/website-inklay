using Conesoft.Hosting;
using Conesoft.Website.Inklay.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLoggingToHost();
builder.Services.AddHttpClient();
builder.Services.AddPeriodicGarbageCollection(TimeSpan.FromMinutes(1));
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

app.MapRazorComponents<Conesoft.Website.Inklay.Components.App>();

app.Run();

