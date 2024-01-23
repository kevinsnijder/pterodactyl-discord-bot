using Discord;
using Discord.Interactions;
using pterodactyl.Utility;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Preconditions
{
   /// <summary>
   /// Requires the user to be logged in
   /// </summary>
   public class RequiresAnyLogin : PreconditionAttribute
   {
      public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
      {
         var hasGlobalKey = !string.IsNullOrEmpty(Settings.GlobalPterodactylKey);

         if (hasGlobalKey)
            return PreconditionResult.FromSuccess();

         return await new RequiresNonGlobalLogin().CheckRequirementsAsync(context, commandInfo, services);
      }
   }
}
