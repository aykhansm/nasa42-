public class MongoDBSettings
{
    public string GameCollectionName { get; set; } = string.Empty; // Default empty string to ensure non-null
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
