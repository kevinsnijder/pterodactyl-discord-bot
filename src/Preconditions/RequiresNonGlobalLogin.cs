using Discord;
using Discord.Interactions;
using pterodactyl.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Preconditions
{
   /// <summary>
   /// Requires the user to be logged in as a non-global user
   /// </summary>
   public class RequiresNonGlobalLogin : PreconditionAttribute
   {
      private PterodactylDatabase _database;

      public RequiresNonGlobalLogin()
      {
         _database = PterodactylDatabase.Instance;
      }

      public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
      {
         var currentUser = _database.GetUsers().Where(user => user.DiscordID == (long)context.Interaction.User.Id).FirstOrDefault();

         if (currentUser == null || string.IsNullOrEmpty(currentUser.PterodactylApiKey))
         {
            var errormessage = Messages.Get("login.required");
            await context.Interaction.RespondAsync(errormessage, ephemeral: true);
            return PreconditionResult.FromError(errormessage);
         }

         return PreconditionResult.FromSuccess();
      }
   }
}
