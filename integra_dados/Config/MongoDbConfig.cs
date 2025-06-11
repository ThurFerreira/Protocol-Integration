namespace integra_dados.Config;

public class MongoDbConfig
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
}