using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace prueba.Services
{
    public interface IParticipantsService
    {
        // Return raw JSON documents from the Participantes container
        Task<string[]> GetParticipantesAsync();
    }

    public class ParticipantsService : IParticipantsService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<ParticipantsService> _logger;
        private readonly string _databaseId;
        private readonly string _containerId;

        public ParticipantsService(CosmosClient cosmosClient, IConfiguration config, ILogger<ParticipantsService> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
            // Read database and container names from configuration (ConnectionStrings2 section)
            _databaseId = config["ConnectionStrings2:CosmosDb_Database"] ?? string.Empty;
            _containerId = config["ConnectionStrings2:CosmosDb_Container"] ?? string.Empty;
        }

        public async Task<string[]> GetParticipantesAsync()
        {
            if (string.IsNullOrWhiteSpace(_databaseId) || string.IsNullOrWhiteSpace(_containerId))
            {
                _logger.LogWarning("Cosmos database or container not configured");
                return Array.Empty<string>();
            }

            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            var results = new List<string>();

            try
            {
                // Use the streaming iterator to read raw JSON and avoid provider-specific materialization issues
                var iterator = container.GetItemQueryStreamIterator(new QueryDefinition("SELECT * FROM c"));
                while (iterator.HasMoreResults)
                {
                    using var response = await iterator.ReadNextAsync();
                    response.EnsureSuccessStatusCode();

                    // Read response content as JSON and extract the Documents array
                    using var doc = JsonDocument.Parse(response.Content);
                    if (doc.RootElement.TryGetProperty("Documents", out var docs) && docs.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in docs.EnumerateArray())
                        {
                            results.Add(item.GetRawText());
                        }
                    }
                    else
                    {
                        // If structure is unexpected, add the whole response
                        results.Add(doc.RootElement.GetRawText());
                    }
                }
            }
            catch (CosmosException cex)
            {
                _logger.LogError(cex, "Cosmos error loading participantes: {StatusCode} - {Message}", cex.StatusCode, cex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading participantes from Cosmos (stream)");
                throw;
            }

            return results.ToArray();
        }
    }
}
