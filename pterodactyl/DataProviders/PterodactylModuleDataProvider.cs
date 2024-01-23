using pterodactyl.DataObjects;
using pterodactyl.Services;
using Microsoft.Extensions.Logging;
using pterodactyl.Storage;
using pterodactyl.Utility;

namespace pterodactyl.DataProviders
{
   /// <summary>
   /// Provides data to the PterodactylModule and caches it to prevent bombaring my server with requests
   /// </summary>
   public class PterodactylModuleDataProvider : IPterodactylModuleDataProvider
   {
      private readonly ILogger<PterodactylModuleDataProvider> _logger;
      private readonly PterodactylDatabase _database;
      private readonly IPterodactylHttpService _pterodactylHttpService;

      /// <summary>
      /// Constructor
      /// </summary>
      public PterodactylModuleDataProvider(IPterodactylHttpService pterodactylHttpService, ILogger<PterodactylModuleDataProvider> logger)
      {
         _pterodactylHttpService = pterodactylHttpService;
         _logger = logger;
         _database = PterodactylDatabase.Instance;
      }

      /// <inheritdoc/>
      public async Task<IEnumerable<PterodactylServerDto>> GetServersAsync(ulong interactionId, ulong userId)
      {
         var logPrefix = "interaction:" + interactionId + ", user:" + userId + " | ";

         var personalServers = await GetPersonalServersAsync(interactionId, userId);
         var globalServers = await GetGlobalServersAsync(interactionId, userId);

         if (personalServers.Any() && globalServers.Any())
         {
            _logger.LogInformation(logPrefix + "Removing duplicates");
            globalServers = globalServers.Where(server => !personalServers.Any(pserver => pserver.Identifier == server.Identifier));
         }
         var totalServers = personalServers.Concat(globalServers);

         _logger.LogInformation(logPrefix + "Returning servers");
         return totalServers.OrderBy(server => server.Name);
      }

      /// <inheritdoc/>
      public async Task<bool> SendServerSignalAsync(ulong interactionId, ulong userId, string serverID, Signals signal)
      {
         var logPrefix = "interaction:" + interactionId + ", user:" + userId + " | ";
         var signalName = Enum.GetName(typeof(Signals), signal) ?? "";

         var personalKey = GetPersonalApiKey(userId);

         if (personalKey != null)
         {
            var personalServers = await GetPersonalServersAsync(interactionId, userId);

            if (personalServers.Any(pserver => pserver.Identifier == serverID))
            {
               _logger.LogInformation(logPrefix + "Sending server signal with personal API key");
               return await _pterodactylHttpService.SendSignalAsync(personalKey, serverID, signalName);
            }
         }
        
         _logger.LogInformation(logPrefix + "Sending server signal with global API key");
         var globalKey = Settings.GlobalPterodactylKey;

         if (globalKey == null)
         {
            _logger.LogInformation(logPrefix + "No global API key is set");
            return false; ;
         }

         return await _pterodactylHttpService.SendSignalAsync(globalKey, serverID, signalName);
      }

      private async Task<IEnumerable<PterodactylServerDto>> GetPersonalServersAsync(ulong interactionId, ulong userId)
      {
         var logPrefix = "interaction:" + interactionId + ", user:" + userId + " | ";
         _logger.LogInformation(logPrefix + "Getting all personal servers");

         var serverList = new List<PterodactylServerDto>();

         var personalKey = GetPersonalApiKey(userId);
         if (personalKey == null)
         {
            _logger.LogInformation(logPrefix + "No personal login set, skipping pterodactyl request");
            return serverList;
         }

         _logger.LogInformation(logPrefix + "Personal information set, doing pterodactyl request");
         var servers = await _pterodactylHttpService.GetServersAsync(personalKey);

         if (servers == null)
         {
            _logger.LogInformation(logPrefix + "Something went wrong when trying to retrieve the personal servers from pterodactyl.");
            return serverList;
         }

         foreach (var server in servers.data)
         {
            serverList.Add(new PterodactylServerDto()
            {
               Identifier = server.attributes.identifier,
               Uuid = server.attributes.uuid,
               Name = server.attributes.name,
               Description = server.attributes.description,
               UsedApiKey = personalKey
            });
         }

         return serverList;
      }

      private async Task<IEnumerable<PterodactylServerDto>> GetGlobalServersAsync(ulong interactionId, ulong userId)
      {
         var logPrefix = "interaction:" + interactionId + ", user:" + userId + " | ";
         _logger.LogInformation(logPrefix + "Getting all global servers");

         var serverList = new List<PterodactylServerDto>();

         var globalKey = Settings.GlobalPterodactylKey;
         if (!string.IsNullOrEmpty(globalKey))
         {
            _logger.LogInformation(logPrefix + "Global information set, doing pterodactyl request");

            var servers = await _pterodactylHttpService.GetServersAsync(globalKey);

            if (servers == null)
            {
               _logger.LogInformation(logPrefix + "Something went wrong when trying to retrieve the global servers from pterodactyl.");
               return serverList;
            }

            foreach (var server in servers.data)
            {
               serverList.Add(new PterodactylServerDto()
               {
                  Identifier = server.attributes.identifier,
                  Uuid = server.attributes.uuid,
                  Name = server.attributes.name,
                  Description = server.attributes.description,
                  UsedApiKey = globalKey
               });
            }
         }
         else
            _logger.LogInformation(logPrefix + "No global user set, skipping pterodactyl request");

         return serverList;
      }

      private string? GetPersonalApiKey(ulong userId) 
      {
         var currentUser = _database.GetUsers().Where(user => user.DiscordID == (long) userId).FirstOrDefault();
         if (currentUser == null || string.IsNullOrEmpty(currentUser.PterodactylApiKey))
            return null;

         return currentUser.PterodactylApiKey;
      }
   }
}
