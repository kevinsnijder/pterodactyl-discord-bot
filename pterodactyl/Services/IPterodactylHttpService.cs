using pterodactyl.DataObjects;

namespace pterodactyl.Services
{
   /// <summary>
   /// Interface for getting information from the pterodactyl API
   /// </summary>
   public interface IPterodactylHttpService
   {
      /// <summary>
      /// Gets all servers from the pterodactyl instance
      /// </summary>
      public Task<PterodactylServerListDto> GetServersAsync(string apiKey);
      /// <summary>
      /// Sends a server signal to the specified server
      /// </summary>
      public Task<bool> SendSignalAsync(string apiKey, string serverID, string signal);
      /// <summary>
      /// Gets the resource usage of the specified server
      /// </summary>
      public Task<PterodactylResourceUsageDto> GetResourceUsage(string apiKey, string serverID);
      /// <summary>
      /// Checks if the API key is valid
      /// </summary>
      public Task<bool> CheckLogin(string apiKey);
   }
}
