using DisCatSharp;
using YoutubeDiscordBot.commands;
using YoutubeDiscordBot.config;
using Microsoft.Extensions.Logging;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Interactivity;
using DisCatSharp.EventArgs;


namespace YoutubeDiscordBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextExtension Commands {  get; set; }

        static async Task Main(string[] args)
        {
            var config = BotConfig.FromEnvironment();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = "Enter Token Here",
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                HttpTimeout = TimeSpan.FromSeconds(10)
            };

            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.Ready += Client_Ready;
            Client.ClientErrored += Client_ClientErrored;


            var slash = Client.UseApplicationCommands();

            slash.RegisterGlobalCommands<SlashCommands>();
            Console.WriteLine("Slash commands registered successfully.");


            var endpoint = new ConnectionEndpoint
            {
                Hostname = "lava-v4.ajieblogs.eu.org",
                Port = 443,
                Secured = true,
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "https://dsc.gg/ajidevserver",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Client.UseLavalink();

            await Client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            
            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }

        private static Task Client_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            Console.WriteLine($"An error occurred: {e.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}


