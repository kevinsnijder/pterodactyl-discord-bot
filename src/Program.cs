using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pterodactyl.DataProviders;
using pterodactyl.Services;
using System;
using discord.Services;
using pterodactyl.Utility;

Console.WriteLine("----------------------------------------------------------------------");
Console.WriteLine(" _  __          _             ____  _                 ____        _   ");
Console.WriteLine("| |/ /_____   _(_)_ __  ___  |  _ \\| |_ ___ _ __ ___ | __ )  ___ | |_ ");
Console.WriteLine("| ' // _ \\ \\ / / | '_ \\/ __| | |_) | __/ _ \\ '__/ _ \\|  _ \\ / _ \\| __|");
Console.WriteLine("| . \\  __/\\ V /| | | | \\__ \\ |  __/| ||  __/ | | (_) | |_) | (_) | |_ ");
Console.WriteLine("|_|\\_\\___| \\_/ |_|_| |_|___/ |_|    \\__\\___|_|  \\___/|____/ \\___/ \\__|");
Console.WriteLine("----------------------------------------------------------------------");

var discordToken = Settings.DiscordToken;
var url = Settings.PterodactylUrl;
var authGroup = Settings.DiscordAuthGroup;
var globalKey = Settings.GlobalPterodactylKey;

if (string.IsNullOrEmpty(authGroup))
   Console.WriteLine("Currently not adding users to a special discord group on authentication.");
else
   Console.WriteLine("Currently adding users to a special discord group on authentication. (Group ID "+ authGroup + ")");

if (string.IsNullOrEmpty(globalKey))
   Console.WriteLine("Currently not using a global API key that everyone can use.");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
       var discordConfig = new DiscordSocketConfig()
       {
          GatewayIntents = Discord.GatewayIntents.Guilds
       };

       services.AddSingleton(new DiscordSocketClient(discordConfig));
       services.AddSingleton<InteractionService>();
       services.AddScoped<IPterodactylModuleDataProvider, PterodactylModuleDataProvider>();
       services.AddHostedService<InteractionHandlingService>();
       services.AddHostedService<DiscordStartupService>();
       services.AddHttpClient<IPterodactylHttpService, PterodactylHttpService>(client =>
       {
          var address = Settings.PterodactylUrl;
          client.BaseAddress = new Uri(address);
       });

    })
    .Build();

await host.RunAsync();