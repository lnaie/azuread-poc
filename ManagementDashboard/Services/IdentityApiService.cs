
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementDashboard.Services
{
    /// <summary>
    /// Identity API service implementation.
    /// </summary>
    public class IdentityApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IdentityApiService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public IdentityApiService(IHttpClientFactory clientFactory,  ILogger<IdentityApiService> logger)
        {
            _httpClientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<List<TodoItem>> GetAllTodos(CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient("IdentityApi");
            var response = await httpClient.GetAsync("/api/todolist/all", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string text = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return JsonSerializer.Deserialize<List<TodoItem>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }

            throw new System.Exception($"Failed to load. API response status code: {response.StatusCode}.");
            //return new List<TodoItem>(0);
        }

        public async Task<List<ClaimItem>> GetApiClaims(CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient("IdentityApi");
            var response = await httpClient.GetAsync("/api/users/identity", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string text = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return JsonSerializer.Deserialize<List<ClaimItem>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }

            throw new System.Exception($"Failed to load. API response status code: {response.StatusCode}.");
            //return new List<ClaimItem>(0);
        }
    }

    public class TodoItem
    {
        public string Title { get; set; }
        public string Owner { get; set; }
    }

    public class ClaimItem
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
