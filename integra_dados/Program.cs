using integra_dados.Config;
using integra_dados.Models;
using integra_dados.Repository;
using integra_dados.Services;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Modbus
builder.Services.AddSingleton<ModbusApi>();

// Kafka
builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<KafkaService>();

// MongoDB
var mongoSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbConfig>();
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = mongoSettings.ConnectionString;
    return new MongoClient(connectionString);
});

builder.Services.AddScoped(s =>
{
    var client = s.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(mongoSettings.DatabaseName);
    return database.GetCollection<SupervisoryRegistry>(nameof(SupervisoryRegistry));
});

builder.Services.AddScoped<ISupervisoryRepository, SupervisoryRepository>();
builder.Services.AddScoped<SupervisoryService>();

// Http server config
var serverSettings = builder.Configuration.GetSection("HttpServer").Get<ServerConfig>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(serverSettings.Port);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOtherApp", policy =>
    {
        policy.AllowAnyOrigin()     // Permite qualquer origem
            .AllowAnyMethod()     // Permite qualquer método HTTP
            .AllowAnyHeader();    // Permite qualquer header
    });
});

builder.Services.AddControllers();

// Background Services
builder.Services.AddHostedService<SupervisoryScheduler>();

// Adiciona Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
