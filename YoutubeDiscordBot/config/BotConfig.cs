using Newtonsoft.Json;

namespace YoutubeDiscordBot.config
{
    internal class BotConfig
    {
        public string token { get; set; }
        public string prefix { get; set; }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure obj = JsonConvert.DeserializeObject<JSONStructure>(json);

                this.token = obj.token;
                this.prefix = obj.prefix;
            }
        }
    }

    internal sealed class JSONStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}
