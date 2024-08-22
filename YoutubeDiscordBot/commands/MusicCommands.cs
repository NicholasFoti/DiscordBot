using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace YoutubeDiscordBot.commands
{
    public class MusicCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task MyFirstCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Fuck off {ctx.User.Username}");
        }

        [Command("play")]
        public async Task PlayCommand(CommandContext ctx, [RemainingText] string search)
        {
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"You have to be in a VC retard",
                };
                await ctx.Channel.SendMessageAsync(embed: notInVCEmbed);
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection not established");
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Enter a valid VC retard");
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(ctx.Member.VoiceState.Channel);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Lavalink failed to connect");
                return;
            }

            var searchQuery = await node.Rest.GetTracksAsync(search);
            if(searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await ctx.Channel.SendMessageAsync($"Failed to find music for {search}");
                return;
            }

            var musicTrack = searchQuery.Tracks.First();

            await conn.PlayAsync(musicTrack);

            var youtubeClient = new YoutubeClient();
            var video = await youtubeClient.Videos.GetAsync(musicTrack.Identifier);
            var thumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url;

            string musicDescription = $"**🎵 Banger Playing:** {musicTrack.Title} \n" +
                                      $"**⏱ Duration:** {musicTrack.Length.Minutes}:{musicTrack.Length.Seconds:D2} \n" +
                                      $"**🔗 URL for kane to use in a YouTube edit:**({musicTrack.Uri})";

            var thumbnailEmbed = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = thumbnailUrl
            };

            var footerEmbed = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{ctx.Member.DisplayName}'s song",
                IconUrl = ctx.User.AvatarUrl
            };

            var nowPlayingEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = $"🎶 Enjoy your music... You filthy animal 🎶 \n",
                Description = musicDescription,
                ImageUrl = thumbnailUrl,
                Footer = footerEmbed
            };

            await ctx.Channel.SendMessageAsync(embed: nowPlayingEmbed);
        }

        [Command("stop")]
        public async Task StopCommand(CommandContext ctx)
        {
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"You have to be in a VC retard",
                };
                await ctx.Channel.SendMessageAsync(embed: notInVCEmbed);
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection not established");
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Enter a valid VC retard");
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Im not even in the channel bruh");
                return;
            }

            if(conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There isnt even a song playing...");
                return;
            }

            await conn.StopAsync();
            await conn.DisconnectAsync();

            var stopEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Red,
                Title = "Stopped"
            };

            await ctx.Channel.SendMessageAsync(embed: stopEmbed);
        }

        [Command("pause")]
        public async Task PauseCommand(CommandContext ctx)
        {
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"You have to be in a VC retard",
                };
                await ctx.Channel.SendMessageAsync(embed: notInVCEmbed);
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection not established");
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Enter a valid VC retard");
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Im not even in the channel bruh");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There isnt even a song playing...");
                return;
            }

            await conn.PauseAsync();

            var pauseEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Yellow,
                Title = "Paused"
            };

            await ctx.Channel.SendMessageAsync(embed: pauseEmbed);
        }

        [Command("resume")]
        public async Task ResumeCommand(CommandContext ctx)
        {
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"You have to be in a VC retard",
                };
                await ctx.Channel.SendMessageAsync(embed: notInVCEmbed);
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection not established");
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Enter a valid VC retard");
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Im not even in the channel bruh");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There isnt even a song playing...");
                return;
            }

            await conn.ResumeAsync();

            var resumeEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = "Resumed"
            };

            await ctx.Channel.SendMessageAsync(embed: resumeEmbed);
        }
    }
}
