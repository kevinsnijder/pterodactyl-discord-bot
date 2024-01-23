using Discord;
using Discord.Interactions;
using DiscordBot.AutoCompleteHandlers;
using DiscordBot.Preconditions;
using Microsoft.Extensions.Logging;
using pterodactyl.Extensions;
using pterodactyl.DataObjects;
using pterodactyl.DataProviders;
using pterodactyl.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using pterodactyl;

namespace DiscordBot.Modules
{
   /// <summary>
   /// Module loaded by the discord chatbot that provides functions for controlling Pterodactyl
   /// </summary>
   public class PterodactylModule : InteractionModuleBase<SocketInteractionContext>
   {
      private ILogger<PterodactylModule> _logger;
      private IPterodactylModuleDataProvider _dataProvider;
      private IPterodactylHttpService _pterodactylService;

      /// <summary>
      /// Constructor
      /// </summary>
      public PterodactylModule(ILogger<PterodactylModule> logger, IPterodactylModuleDataProvider dataProvider, IPterodactylHttpService pterodactylHttpService)
      {
         _logger = logger;
         _dataProvider = dataProvider;
         _pterodactylService = pterodactylHttpService;
      }

      /// <summary>
      /// Starts a server on a pterodactyl panel
      /// </summary>
      [SlashCommand("listservers", "Lists all available servers.")]
      [RequiresAnyLogin]
      [RequireContext(ContextType.Guild)]
      [RequireUserPermission(GuildPermission.UseApplicationCommands)]
      public Task ListServersCommand()
      {
         var interactionId = Context.Interaction.Id;
         var userId = Context.Interaction.User.Id;
         var logPrefix = "interaction:" + interactionId + ", user:" + userId + " | ";

         _logger.LogInformation(logPrefix + string.Format(Messages.Get("channel.command.executed"), Context.User.Username, "/listservers", Context.Interaction.Channel.Name, Context.Interaction.ChannelId));
         var servers = _dataProvider.GetServersAsync(interactionId, userId).Result.WithStatus(_pterodactylService);

         if (!servers.Any())
            return RespondAsync(Messages.Get("listservers.noservers"), ephemeral: true);

         var onlineServers = servers.FilterOnline();
         var offlineServers = servers.FilterOffline();

         StringBuilder sb = new StringBuilder();
         sb.AppendLine(Messages.Get("listservers.header"));

         if (onlineServers.Any())
         {
            sb.AppendLine(Messages.Get("listservers.header.online"));
            foreach (var server in onlineServers)
            {
               sb.AppendLine(string.Format(Messages.Get("listservers.item." + server.Status), server.Name));
            }
            sb.AppendLine("");
         }

         if (offlineServers.Any())
         {
            sb.AppendLine(Messages.Get("listservers.header.offline"));
            foreach (var server in offlineServers)
            {
               sb.AppendLine(string.Format(Messages.Get("listservers.item." + server.Status), server.Name));
            }
         }
         _logger.LogInformation(logPrefix + "Sending response to discord");
         // Send the formatted list as a reply
         return RespondAsync(sb.ToString(), ephemeral: true);
      }

      /// <summary>
      /// Starts a server on a pterodactyl panel
      /// </summary>
      [SlashCommand("startserver", "Starts an offline server.")]
      [RequiresAnyLogin]
      [RequireContext(ContextType.Guild)]
      [RequireUserPermission(GuildPermission.UseApplicationCommands)]
      public Task StartServerCommand([Autocomplete(typeof(PterodactylServersAutoCompleteHandler))] string serverID)
      {
         _logger.LogInformation(Context.Interaction.Id + " | " + string.Format(Messages.Get("channel.command.executed"), Context.User.Username, "/startserver " + serverID, Context.Interaction.Channel.Name, Context.Interaction.ChannelId));
         _logger.LogInformation(Context.Interaction.Id + " | " + "Sending startserver command to pterodactyl");
         RespondAsync(string.Format(Messages.Get("startserver.starting"), serverID), ephemeral: true);

         var startresult = _dataProvider.SendServerSignalAsync(Context.Interaction.Id, Context.Interaction.User.Id, serverID, Signals.start).Result;

         if (startresult == false)
         {
            _logger.LogError(Context.Interaction.Id + " | " + "Failed to send the startserver command to pterodactyl");
            return ModifyOriginalResponseAsync(msg => msg.Content = string.Format(Messages.Get("startserver.failed"), serverID, 0));
         }

         var server = GetServerByIdAsync(serverID).Result;
         var serverName = server.Name;
         server.Status = "starting";

         var startTime = DateTime.Now.AddSeconds(-1);
         var maxEndTime = startTime.AddSeconds(120);
         var totalSeconds = (int)(DateTime.Now - startTime).TotalSeconds;

         _logger.LogInformation(Context.Interaction.Id + " | " + "Creating status update loop");
         while (DateTime.Now < maxEndTime && server.Status == "starting")
         {
            totalSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
            if (totalSeconds > 15 && totalSeconds % 2 == 0)
               server.WithStatus(_pterodactylService);

            ModifyOriginalResponseAsync(msg => msg.Content = string.Format(Messages.Get("startserver.startingCount"), serverName, totalSeconds));
            Thread.Sleep(1000 - DateTime.Now.Millisecond);
         }

         _logger.LogInformation(Context.Interaction.Id + " | " + "Server status changed to " + server.Status);
         _logger.LogInformation(Context.Interaction.Id + " | " + "Finishing /startserver command");

         if (server.Status == "running")
            return ModifyOriginalResponseAsync(msg => msg.Content = string.Format(Messages.Get("startserver.started"), serverName, totalSeconds));
         else if (server.Status == "starting")
            return ModifyOriginalResponseAsync(msg => msg.Content = string.Format(Messages.Get("startserver.timeout"), serverName, totalSeconds));
         else
            return ModifyOriginalResponseAsync(msg => msg.Content = string.Format(Messages.Get("startserver.failed"), serverName, totalSeconds));
      }

      /// <summary>
      /// Stops a server on a pterodactyl panel
      /// </summary>
      [SlashCommand("stopserver", "Stops a running server.")]
      [RequiresAnyLogin]
      [RequireContext(ContextType.Guild)]
      [RequireUserPermission(GuildPermission.UseApplicationCommands)]
      public Task StopServerCommand([Autocomplete(typeof(PterodactylServersAutoCompleteHandler))] string serverID)
      {
         _logger.LogInformation(Context.Interaction.Id + " | " + string.Format(Messages.Get("channel.command.executed"), Context.User.Username, "/stopserver " + serverID, Context.Interaction.Channel.Name, Context.Interaction.ChannelId));
         _logger.LogInformation(Context.Interaction.Id + " | " + "Sending stopserver command to pterodactyl");

         RespondAsync(string.Format(Messages.Get("stopserver.stopping"), serverID), ephemeral: true);
         var stopresult = _dataProvider.SendServerSignalAsync(Context.Interaction.Id, Context.Interaction.User.Id, serverID, Signals.stop).Result;

         if (stopresult == false)
         {
            _logger.LogError(Context.Interaction.Id + " | " + "Failed to send the stopserver command to pterodactyl");
            return ModifyOriginalResponseAsync(msg => msg.Content = string.Format(Messages.Get("stopserver.failed"), serverID));
         }

         var server = GetServerByIdAsync(serverID).Result;
         var serverName = server.Name;

         _logger.LogInformation(Context.Interaction.Id + " | " + "Finishing /stopserver command");
         return ModifyOriginalResponseAsync(msg => msg.Content = string.Format(Messages.Get("stopserver.stopped"), serverName));
      }

      /// <summary>
      /// Sets the current channel as a console channel for the specified server
      /// </summary>
      //[SlashCommand("setconsolechannel", "Sets the current channel as a console channel for the specified server.")]
      //[RequiresAnyLogin]
      //[RequireContext(ContextType.Guild)]
      //[RequireUserPermission(GuildPermission.UseApplicationCommands)]
      //public Task SetConsoleChannelCommand([Autocomplete(typeof(PterodactylServersAutoCompleteHandler))] string serverID, bool forwardChatToConsole = true)
      //{
      //   _logger.LogInformation(Context.Interaction.Id + " | " + string.Format(Messages.Get("channel.command.executed"), Context.User.Username, "/setconsolechannel " + serverID, Context.Interaction.Channel.Name, Context.Interaction.ChannelId));

      //}

      private async Task<PterodactylServerDto> GetServerByIdAsync(string serverID)
      {
         var servers = await _dataProvider.GetServersAsync(Context.Interaction.Id, Context.Interaction.User.Id);
         var server = servers.FirstOrDefault(srv => srv.Identifier.ToLower() == serverID.ToLower());
         return server;
      }
   }
}
