using integra_dados.Config;
using integra_dados.Models;
using integra_dados.Repository;
using integra_dados.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// MongoDB
var mongoSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbConfig>();
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
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

builder.Services.AddControllers();

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

// app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();

// app.UseAuthorization();

app.MapControllers();

app.Run();
