using integra_dados.Config;
using integra_dados.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace integra_dados.Repository;

public static class MongoServiceCollectionExtensions
{
    public static IServiceCollection AddMongoCollection<T>(this IServiceCollection services)
    {
        services.AddSingleton<IMongoCollection<T>>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var settings = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
            var database = client.GetDatabase(settings.DatabaseName);
            var collectionName = (typeof(T).GetCustomAttributes(typeof(BsonCollectionAttribute), true)
                                     .FirstOrDefault() as BsonCollectionAttribute)?.CollectionName ?? typeof(T).Name;

            return database.GetCollection<T>(collectionName);
        });

        return services;
    }
}