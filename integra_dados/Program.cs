using integra_dados.Config;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration
        .GetSection("MongoDb").Get<MongoDbConfig>();
    return new MongoClient(settings.ConnectionString);
});

// Http server config
var serverSettings = builder.Configuration.GetSection("HttpServer").Get<ServerConfig>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(serverSettings.Port); // Escuta em todas as interfaces de rede, porta 5000
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
