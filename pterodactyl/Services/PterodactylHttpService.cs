using Microsoft.Extensions.Logging;
using pterodactyl.DataObjects;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace pterodactyl.Services
{
   /// <summary>
   /// Service for doing network requests to pterodactyl
   /// </summary>
   public class PterodactylHttpService : IPterodactylHttpService
   {
      private HttpClient _httpClient;

      private readonly string _getServersPath = "/api/client";
      private readonly string _serverSignalPath = "/api/client/servers/{0:G}/power";
      private readonly string _resourceUsagePath = "/api/client/servers/{0:G}/resources";
      private readonly string _accountDetailsPath = "/api/client/account";
      private readonly ILogger<PterodactylHttpService> _logger;

      /// <summary>
      /// Constructor
      /// </summary>
      public PterodactylHttpService(HttpClient httpClient, ILogger<PterodactylHttpService> logger)
      {
         _logger = logger;
         _httpClient = httpClient;
      }

      /// <inheritdoc/>
      public async Task<PterodactylServerListDto> GetServersAsync(string apiKey)
      {
         SetApiKey(apiKey);

         _logger.LogInformation("Getting servers from pterodactyl");
         var result = await _httpClient.GetAsync(_getServersPath);
         if (!result.IsSuccessStatusCode)
            throw new ArgumentNullException("An unsuccessful status code returned from the GetServersAsync call.");

         var stringresult = await result.Content.ReadAsStringAsync();
         var serverList = JsonSerializer.Deserialize<PterodactylServerListDto>(stringresult);

         if (serverList == null)
            throw new ArgumentNullException("Could not deserialize the result of the GetServersAsync call.");

         serverList.data = serverList.data.Where(data => !data.attributes.is_suspended && !data.attributes.is_installing).ToArray();
         return serverList;
      }

      /// <inheritdoc/>
      public async Task<bool> SendSignalAsync(string apiKey, string serverID, string signal)
      {
         SetApiKey(apiKey);

         _logger.LogInformation("Sending signal to server");
         var path = string.Format(_serverSignalPath, serverID);
         var stringContent = "{\"signal\": \"" + signal + "\"}";
         var result = await _httpClient.PostAsync(path, new StringContent(stringContent, Encoding.UTF8, "application/json"));
         return result.IsSuccessStatusCode;
      }

      /// <inheritdoc/>
      public async Task<PterodactylResourceUsageDto> GetResourceUsage(string apiKey, string serverID)
      {
         SetApiKey(apiKey);

         _logger.LogInformation("Getting resource usage for server " + serverID);
         var path = string.Format(_resourceUsagePath, serverID);
         var result = await _httpClient.GetAsync(path);

         if (!result.IsSuccessStatusCode)
            throw new ArgumentNullException("An unsuccessful status code returned from the GetResourceUsage call.");

         var stringresult = await result.Content.ReadAsStringAsync();
         var resourceUsage = JsonSerializer.Deserialize<PterodactylResourceUsageDto>(stringresult);

         if (resourceUsage == null)
            throw new ArgumentNullException("Could not deserialize the result of the GetResourceUsage call.");

         return resourceUsage;
      }

      /// <inheritdoc/>
      private void SetApiKey(string apiKey)
      {
         _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
         return;
      }

      /// <inheritdoc/>
      public async Task<bool> CheckLogin(string apiKey)
      {
         SetApiKey(apiKey);

         _logger.LogInformation("Checking valid api key");
         var result = await _httpClient.GetAsync(_accountDetailsPath);
         return result.IsSuccessStatusCode && !(await result.Content.ReadAsStringAsync()).StartsWith("<");
      }
   }
}
