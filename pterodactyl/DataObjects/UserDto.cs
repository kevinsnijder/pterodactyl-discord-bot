namespace pterodactyl.DataObjects
{
   public record UserDto
   {
      public int Id { get; set; }
      public long DiscordID { get; set; }
      public string? PterodactylApiKey { get; set; }
   }
}
