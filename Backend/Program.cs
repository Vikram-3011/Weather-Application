using Backend.Configuration;
using WeatherApi.Models;
using WeatherApi.Services;
using static LocationController;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<OpenCageSettings>(builder.Configuration.GetSection("OpenCage"));
builder.Services.Configure<SupabaseConfig>(
    builder.Configuration.GetSection("Supabase"));


builder.Services.Configure<WeatherSettings>(
    builder.Configuration.GetSection("Weather"));


builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<FavoriteCityService>();
builder.Services.AddSingleton<PersonalInfoService>();
builder.Services.AddSingleton<AlertPreferenceService>();
builder.Services.AddMemoryCache();



builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7297") // Replace with your WASM port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.UseDeveloperExceptionPage();

app.Run();
