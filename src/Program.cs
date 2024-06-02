using Conesoft.Website.Inklay.Components;
using Conesoft.Website.Inklay.Services;
using Conesoft.Website.Inklay.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddSingletonWith<CalendarCache>(TimeSpan.FromMinutes(5));
builder.Services.AddSingletonWith<WeatherCache>(TimeSpan.FromMinutes(30));
builder.Services.AddSingletonWith<GarbageCollect>(TimeSpan.FromMinutes(15));
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

