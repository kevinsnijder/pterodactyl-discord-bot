#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace pterodactyl.DataObjects
{
   /// <summary>
   /// Dataobject for retrieving pterodactyl resource usage data
   /// </summary>
   public record PterodactylResourceUsageDto
   {
      public string _object { get; set; }
      public PterodactylResourceUsageDtoAttributes attributes { get; set; }
   }

   /// <summary>
   /// Dataobject for retrieving pterodactyl resource usage data attributes
   /// </summary>
   public record PterodactylResourceUsageDtoAttributes
   {
      public string current_state { get; set; }
   }
}