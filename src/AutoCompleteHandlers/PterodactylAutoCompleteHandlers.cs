using Discord;
using Discord.Interactions;
using pterodactyl.DataProviders;
using pterodactyl.Extensions;
using pterodactyl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.AutoCompleteHandlers
{
   /// <summary>
   /// Handles autocompletes for online pterodactyl servers
   /// </summary>
   public class PterodactylServersAutoCompleteHandler : AutocompleteHandler
   {
      private IPterodactylModuleDataProvider _dataProvider;
      private IPterodactylHttpService _pterodactylHttpService;

      /// <summary>
      /// Constructor
      /// </summary>
      public PterodactylServersAutoCompleteHandler(IPterodactylModuleDataProvider dataProvider, IPterodactylHttpService pterodactylHttpService)
      {
         _dataProvider = dataProvider;
         _pterodactylHttpService = pterodactylHttpService;
      }

      /// <inheritdoc/>
      public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
      {
         var commandName = parameter.Command.Name;
         var userTypingString = (string)autocompleteInteraction.Data.Options.First().Value;
         var results = new List<AutocompleteResult>();
         var servers = await _dataProvider.GetServersAsync(context.Interaction.Id, context.Interaction.User.Id);

         servers = servers.WithStatus(_pterodactylHttpService).ToList();

         foreach (var server in servers)
         {
            switch (commandName) {
               case "stopserver":
                  if (server.Status == "running" || server.Status == "starting")
                     results.Add(new AutocompleteResult(server.Name, server.Identifier));
                  break;
               case "startserver":
                  if (server.Status == "stopping" || server.Status == "offline")
                     results.Add(new AutocompleteResult(server.Name, server.Identifier));
                  break;
               case "setconsolechannel":
                  results.Add(new AutocompleteResult(server.Name, server.Identifier));
                  break;
            }
         }

         return AutocompletionResult.FromSuccess(results.OrderBy(x => x.Name).Take(25));// max 25 suggestions at a time (API limit)
      }
   }
}
