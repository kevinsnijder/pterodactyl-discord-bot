#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace pterodactyl.DataObjects
{
   /// <summary>
   /// Basic information about a pterodactyl server
   /// </summary>
   public record PterodactylServerDto
   {
      public string Identifier { get; set; }
      public string Uuid { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public string Status { get; set; }
      public string UsedApiKey { get; set; }
   }
}
