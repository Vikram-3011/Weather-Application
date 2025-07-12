using Frontend.Services;
using Frontend;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<UserStateManager>();
builder.Services.AddScoped<WeatherHistoryService>();
builder.Services.AddScoped<WeatherCompareService>();
builder.Services.AddScoped<FavoriteCityService>();
builder.Services.AddScoped<PersonalInfoService>();
builder.Services.AddScoped<WeatherAlertService>();
builder.Services.AddScoped<AlertPreferenceService>();


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7172/") });

await builder.Build().RunAsync();
