using integra_dados.Config;
using integra_dados.Models;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;
using integra_dados.Models.SupervisoryModel.RegistryModel.OPCUA;
using integra_dados.Repository;
using integra_dados.Services;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using integra_dados.Services.Notifier;
using integra_dados.Supervisory.OPC;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Http Client
builder.Services.AddHttpClient<HttpService>();

// Http server config
var serverSettings = builder.Configuration.GetSection("HttpServer").Get<ServerConfig>();
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(serverSettings.Port); });

// Kafka
var kafkaSettigns = builder.Configuration.GetSection("Kafka");
Console.WriteLine("KAKFA SETTINGS " + kafkaSettigns);
builder.Services.Configure<KafkaConfig>(kafkaSettigns);
builder.Services.AddSingleton<KafkaService>();

// MongoDB Configuration
builder.Services.Configure<MongoDbConfig>(
    builder.Configuration.GetSection("MongoDb")
);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoConn = builder.Configuration["MongoDb:ConnectionString"];
    Console.WriteLine("Mongo Connection String: " + mongoConn);
    if (string.IsNullOrEmpty(mongoConn))
        throw new ArgumentException("MongoDB ConnectionString is null or empty.");
    
    return new MongoClient(mongoConn);
});

// MongoDB Collections
builder.Services.AddMongoCollection<ForecastReadRegistry>();
builder.Services.AddMongoCollection<ModbusReadRegistry>();
builder.Services.AddMongoCollection<OpcReadRegistry>();

// Modbus
builder.Services.AddScoped<IRepository<ModbusReadRegistry>, ModbusRepository>();
builder.Services.AddScoped<ModbusService>();

// Windy Forecast
builder.Services.AddSingleton<WindyApiService>();
builder.Services.AddScoped<ForecastService>();
builder.Services.AddScoped<IRepository<ForecastReadRegistry>, ForecastRepository>();

// OPC
builder.Services.AddScoped<OpcService>();
builder.Services.AddScoped<IRepository<OpcReadRegistry>, OpcRepository>();

// Notifier Dependencies
builder.Services.AddSingleton<Report>();
builder.Services.AddSingleton<ExceptionInfo>();

// Cors
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

// Background Services
builder.Services.AddHostedService<Scheduler>();

// Adiciona Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseCors("AllowOtherApp");

// app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();