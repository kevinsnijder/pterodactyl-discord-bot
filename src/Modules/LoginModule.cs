using Discord;
using Discord.Interactions;
using DiscordBot.AutoCompleteHandlers;
using Microsoft.Extensions.Logging;
using pterodactyl.DataObjects;
using pterodactyl.Services;
using pterodactyl.Storage;
using pterodactyl.Utility;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
   public class LoginModule : InteractionModuleBase<SocketInteractionContext>
   {
      private ILogger<LoginModule> _logger;
      private IPterodactylHttpService _pterodactylService;
      private PterodactylDatabase _database;

      /// <summary>
      /// Constructor
      /// </summary>
      public LoginModule(ILogger<LoginModule> logger, IPterodactylHttpService pterodactylHttpService)
      {
         _logger = logger;
         _pterodactylService = pterodactylHttpService;
         _database = PterodactylDatabase.Instance;
      }

      /// <summary>
      /// Logs you in to the configured pterodactyl panel
      /// </summary>
      [SlashCommand("login", "Logs you in to the configured pterodactyl panel.")]
      [RequireContext(ContextType.Guild)]
      [RequireUserPermission(GuildPermission.UseApplicationCommands)]
      public Task Login(string apiKey)
      {
         var interactionId = Context.Interaction.Id;
         var userId = (long) Context.Interaction.User.Id;
         var logPrefix = "interaction:" + interactionId + ", user:" + userId + " | ";
         _logger.LogInformation(logPrefix + string.Format(Messages.Get("channel.command.executed"), Context.User.Username, "/login", Context.Interaction.Channel.Name, Context.Interaction.ChannelId));

         if(!_pterodactylService.CheckLogin(apiKey).Result)
            return RespondAsync(string.Format(Messages.Get("login.loginnotworking"), Settings.PterodactylUrl), ephemeral: true);
         
         var discordUser = Context.Guild.GetUser(Context.Interaction.User.Id);

         if (!string.IsNullOrEmpty(Settings.DiscordAuthGroup))
         {
            _logger.LogInformation(logPrefix + "Should add role.");
            var roleID = ulong.Parse(Settings.DiscordAuthGroup);
            if (discordUser.Roles.Any(role => role.Id == roleID))
            {
               _logger.LogInformation(logPrefix + "User already has role.");
            }
            else 
            {
               _logger.LogInformation(logPrefix + "Adding role.");
               discordUser.AddRoleAsync(roleID).Wait();
            }
         }

         var user = _database.FindUser(userId);
         if (user == null)
         {
            _database.InsertUser(new UserDto()
            {
               DiscordID = userId,
               PterodactylApiKey = apiKey
            });
            return RespondAsync(string.Format(Messages.Get("login.loginadded"), discordUser?.Nickname ?? user.DiscordID.ToString()), ephemeral: true);
         }
         else
         {
            _database.UpdateUser(new UserDto()
            {
               DiscordID = userId,
               PterodactylApiKey = apiKey
            });

            return RespondAsync(string.Format(Messages.Get("login.loginupdated"), discordUser?.Nickname ?? user.DiscordID.ToString()), ephemeral: true);
         }
      }

      /// <summary>
      /// Removes the API key from a logged in user.
      /// </summary>
      [SlashCommand("deletelogin", "Removes the API key from a logged in user.")]
      [RequireContext(ContextType.Guild)]
      [RequireUserPermission(GuildPermission.UseApplicationCommands)]
      public Task DeleteLogin([Autocomplete(typeof(UsersAutoCompleteHandler))] int userToDelete)
      {
         var interactionId = Context.Interaction.Id;
         var userId = (long)Context.Interaction.User.Id;
         var logPrefix = "interaction:" + interactionId + ", user:" + userId + " | ";
         _logger.LogInformation(logPrefix + string.Format(Messages.Get("channel.command.executed"), Context.User.Username, "/deletelogin", Context.Interaction.Channel.Name, Context.Interaction.ChannelId));

         var user = _database.FindUser(userToDelete);
         if (user == null)
         {
            return RespondAsync(Messages.Get("deletelogin.nonexistent"), ephemeral: true);
         }
         else
         {
            var discordUser = Context.Guild.GetUser(Context.Interaction.User.Id);

            if (!string.IsNullOrEmpty(Settings.DiscordAuthGroup))
            {
               _logger.LogInformation(logPrefix + "Should remove role.");
               var roleID = ulong.Parse(Settings.DiscordAuthGroup);
               if (discordUser.Roles.Any(role => role.Id == roleID))
               {
                  _logger.LogInformation(logPrefix + "Removing role from user.");
                  discordUser.RemoveRoleAsync(roleID).Wait();
               }
               else
               {
                  _logger.LogInformation(logPrefix + "User doesn't have role.");
               }
            }

            if (_database.DeleteUser(user.DiscordID) == 1)
               return RespondAsync(string.Format(Messages.Get("deletelogin.deleted"), discordUser?.Nickname ?? user.DiscordID.ToString()), ephemeral: true);
            else
               return RespondAsync(Messages.Get("deletelogin.nonexistent"), ephemeral: true);
         }
      }
   }
}
