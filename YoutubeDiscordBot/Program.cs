using DSharpPlus;
using DSharpPlus.CommandsNext;
using YoutubeDiscordBot.commands;
using YoutubeDiscordBot.config;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using DSharpPlus.Net;


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
                Token = config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.Ready += Client_Ready;
            Client.ClientErrored += Client_ClientErrored;

            //var commandsConfig = new CommandsNextConfiguration()
            //{
            //    StringPrefixes = new string[] { Environment.GetEnvironmentVariable("COMMAND_PREFIX") },
            //    EnableMentionPrefix = true,
            //    EnableDms = true,
            //    EnableDefaultHelp = true,
            //};

            //Commands = Client.UseCommandsNext(commandsConfig);
            var slash = Client.UseSlashCommands();

            //Commands.RegisterCommands<MusicCommands>();
            slash.RegisterCommands<SlashCommands>();
            Console.WriteLine("Slash commands registered successfully.");

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "lava-v3.ajieblogs.eu.org",
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


