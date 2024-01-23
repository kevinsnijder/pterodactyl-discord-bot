using pterodactyl.DataObjects;
using pterodactyl.Services;

namespace pterodactyl.Extensions
{
   public static class PterodactylServerEnumerableExtensions
   {
      public static IEnumerable<PterodactylServerDto> WithStatus(this IEnumerable<PterodactylServerDto> servers, IPterodactylHttpService pterodactylService)
      {
         var taskList = new List<Task<PterodactylResourceUsageDto>>();
         var serversArray = servers.ToArray();

         for (var i = 0; i < serversArray.Count(); i++)
         {
            var resourceTask = pterodactylService.GetResourceUsage(serversArray[i].UsedApiKey, serversArray[i].Identifier);
            taskList.Add(resourceTask);
         }

         Task.WaitAll(taskList.ToArray());

         for (var y = 0; y < taskList.Count; y++)
         {
            var taskResult = taskList[y].Result;

            if (taskResult == null)
               serversArray[y].Status = "unknown";
            else
               serversArray[y].Status = taskResult.attributes.current_state;
         }

         return servers;
      }

      public static PterodactylServerDto WithStatus(this PterodactylServerDto server, IPterodactylHttpService pterodactylService)
      {
         var resourceTaskResult = pterodactylService.GetResourceUsage(server.UsedApiKey, server.Identifier).Result;
         
         if (resourceTaskResult == null)
            server.Status = "unknown";
         else
            server.Status = resourceTaskResult.attributes.current_state;
         return server;
      }

      public static IEnumerable<PterodactylServerDto> FilterOnline(this IEnumerable<PterodactylServerDto> servers)
      {
         return servers.Where(server => server.Status == "running" || server.Status == "starting").OrderBy(server => server.Name);
      }

      public static IEnumerable<PterodactylServerDto> FilterOffline(this IEnumerable<PterodactylServerDto> servers)
      {
         return servers.Where(server => server.Status != "running" && server.Status != "starting").OrderBy(server => server.Name);
      }
   }
}
