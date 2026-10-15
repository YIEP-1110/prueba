namespace prueba.Configuration
{
    // Typed configuration for Azure Cosmos DB settings as stored under ConnectionStrings2 in appsettings.json
    public class CosmosSettings
    {
        // Raw connection string entry (e.g. "AccountEndpoint=...;AccountKey=...;")
        public string? CosmosDb { get; set; }

        // Database name to use in Cosmos
        public string? CosmosDb_Database { get; set; }
    }
}
