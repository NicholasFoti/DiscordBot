using Newtonsoft.Json;

namespace YoutubeDiscordBot.config
{
    public class BotConfig
    {
        public string Token { get; set; }
        public string Prefix { get; set; }

        public static BotConfig FromEnvironment()
        {
            return new BotConfig
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                Prefix = Environment.GetEnvironmentVariable("COMMAND_PREFIX") ?? "!"
            };
        }
    }
}
