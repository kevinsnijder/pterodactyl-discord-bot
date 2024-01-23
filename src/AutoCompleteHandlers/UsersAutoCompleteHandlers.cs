using Discord;
using Discord.Interactions;
using pterodactyl.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.AutoCompleteHandlers
{
   /// <summary>
   /// Handles autocompletes for online pterodactyl servers
   /// </summary>
   public class UsersAutoCompleteHandler : AutocompleteHandler
   {
      private PterodactylDatabase _database;

      /// <summary>
      /// Constructor
      /// </summary>
      public UsersAutoCompleteHandler()
      {
         _database = PterodactylDatabase.Instance;
      }

      /// <inheritdoc/>
      public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
      {
         var userTypingString = (string)autocompleteInteraction.Data.Options.First().Value;
         // Create a collection with suggestions for autocomplete
         var results = new List<AutocompleteResult>();

         foreach (var user in _database.GetUsers())
         {
            var foundUser = await context.Guild.GetUserAsync((ulong)user.DiscordID);

            if(foundUser != null)
               results.Add(new AutocompleteResult(foundUser.Nickname, user.Id));
            else
               results.Add(new AutocompleteResult(user.DiscordID.ToString(), user.Id));
         }

         if (!string.IsNullOrEmpty(userTypingString))
            results = results.Where(res => res.Name.ToLower().Contains(userTypingString.ToLower())).ToList();

         return AutocompletionResult.FromSuccess(results.Where(users => users.Name.ToLower().Contains(userTypingString)).OrderBy(x => x.Name).Take(25)); // max 25 suggestions at a time (API limit)
      }
   }
}
