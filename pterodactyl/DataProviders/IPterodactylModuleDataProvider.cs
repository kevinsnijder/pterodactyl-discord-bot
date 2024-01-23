using pterodactyl.DataObjects;

namespace pterodactyl.DataProviders
{
   /// <summary>
   /// Interface for providing data to the pterodactylmodule
   /// </summary>
   public interface IPterodactylModuleDataProvider
   {
      /// <summary>
      /// Gets all non-disabled servers from a pterodactyl panel for a specified discord user
      /// </summary>
      public Task<IEnumerable<PterodactylServerDto>> GetServersAsync(ulong interactionId, ulong userId);
      public Task<bool> SendServerSignalAsync(ulong interactionId, ulong userId, string serverID, Signals signal);
   }
}