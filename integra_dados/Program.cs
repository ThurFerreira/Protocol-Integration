using integra_dados.Config;
using integra_dados.Models;
using integra_dados.Repository;
using integra_dados.Services;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Http Client
builder.Services.AddHttpClient<HttpService>();

// Modbus
builder.Services.AddSingleton<ModbusService>();

// Kafka
builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<KafkaService>();

// MongoDB
builder.Services.Configure<MongoDbConfig>(
    builder.Configuration.GetSection("MongoDb")
);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
    if (string.IsNullOrEmpty(settings.ConnectionString))
        throw new ArgumentException("MongoDB ConnectionString is null or empty.");

    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddMongoCollection<ForecastRegistry>();
builder.Services.AddMongoCollection<SupervisoryRegistry>();

builder.Services.AddScoped<IRepository<SupervisoryRegistry>, SupervisoryRepository>();
builder.Services.AddScoped<IRepository<ForecastRegistry>, ForecastRepository>();
builder.Services.AddScoped<SupervisoryService>();
builder.Services.AddScoped<ForecastService>();


// Http server config
var serverSettings = builder.Configuration.GetSection("HttpServer").Get<ServerConfig>();
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(serverSettings.Port); });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOtherApp", policy =>
    {
        policy.AllowAnyOrigin() // Permite qualquer origem
            .AllowAnyMethod() // Permite qualquer método HTTP
            .AllowAnyHeader(); // Permite qualquer header
    });
});

builder.Services.AddControllers();

// Generic Data Service
builder.Services.AddSingleton<GenericDataService>();

// Background Services
builder.Services.AddHostedService<SupervisoryScheduler>();

// Adiciona Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Windy Service
builder.Services.AddSingleton<WindyApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var svc = scope.ServiceProvider.GetRequiredService<WindyApiService>();

    svc.GetWindyForecast(-25.144548639707363, -50.17815056912481, "temp");
}

app.UseCors("AllowOtherApp");

// app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();