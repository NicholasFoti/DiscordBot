using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using YoutubeExplode;
using YoutubeExplode.Common;
using DSharpPlus.SlashCommands;
using DSharpPlus;

namespace YoutubeDiscordBot.commands
{
    internal class SlashCommands : ApplicationCommandModule
    {
        private static Dictionary<ulong, Queue<LavalinkTrack>> _musicQueues = new Dictionary<ulong, Queue<LavalinkTrack>>();

        [SlashCommand("play", "Play audio from YouTube")]
        public async Task PlayCommand(InteractionContext ctx, [Option("query", "YouTube search query or URL")] string search)
        {
            // Acknowledge the interaction and defer the response
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            // Start processing the command
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "You need to be in a voice channel!",
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(notInVCEmbed));
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Lavalink connection is not established."));
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Enter a valid VC retard"));
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(ctx.Member.VoiceState.Channel);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to connect to Lavalink."));
                return;
            }

            var searchQuery = await node.Rest.GetTracksAsync(search);
            if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No matches found for: {search}"));
                return;
            }

            var musicTrack = searchQuery.Tracks.First();

            // Initialize queue for this guild if it doesn't exist
            if (!_musicQueues.ContainsKey(ctx.Guild.Id))
            {
                _musicQueues[ctx.Guild.Id] = new Queue<LavalinkTrack>();
            }

            if (conn.CurrentState.CurrentTrack != null)
            {
                // If a track is currently playing, add the new track to the queue
                _musicQueues[ctx.Guild.Id].Enqueue(musicTrack);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {musicTrack.Title} to the queue."));
            }
            else
            {
                // If no track is playing, play the new track immediately
                await PlayTrack(ctx, conn, musicTrack);
            }
        }
    
        [SlashCommand("stop", "Stop playing audio")]
        public async Task StopCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"You have to be in a VC retard",
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(notInVCEmbed));
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Connection is not established."));
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Enter a valid VC retard"));
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Im not even playing music bruh"));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Im not even playing music bruh"));
                return;
            }

            await conn.StopAsync();
            await conn.DisconnectAsync();

            var stopEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Red,
                Title = "Stopped"
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(stopEmbed));
        }

        [SlashCommand("pause", "Pause audio")]
        public async Task PauseCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"You have to be in a VC retard",
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(notInVCEmbed));
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Connection is not established."));
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Enter a valid VC retard"));
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Im not even playing music bruh"));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Im not even playing music bruh"));
                return;
            }

            await conn.PauseAsync();

            var PauseEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Yellow,
                Title = "Paused"
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(PauseEmbed));
        }

        [SlashCommand("resume", "Resume audio")]
        public async Task ResumeCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"You have to be in a VC retard",
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(notInVCEmbed));
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Connection is not established."));
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Enter a valid VC retard"));
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Im not even playing music bruh"));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Im not even playing music bruh"));
                return;
            }

            await conn.ResumeAsync();

            var resumeEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = "Resumed"
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resumeEmbed));
        }

        [SlashCommand("next", "Skip to the next song in the queue")]
        public async Task NextCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                var notInVCEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "You need to be in a voice channel to use this command!",
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(notInVCEmbed));
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Lavalink connection is not established."));
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The bot is not connected to a voice channel."));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There is no track currently playing."));
                return;
            }

            // Play the next track in the queue if available
            if (_musicQueues.ContainsKey(ctx.Guild.Id) && _musicQueues[ctx.Guild.Id].Count > 0)
            {
                var nextTrack = _musicQueues[ctx.Guild.Id].Dequeue();
                await PlayTrack(ctx, conn, nextTrack);
            }
            else
            {
                // If no more tracks in the queue, disconnect
                await conn.DisconnectAsync();
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No more tracks in the queue. The bot has disconnected."));
            }
        }

        private async Task PlayTrack(InteractionContext ctx, LavalinkGuildConnection conn, LavalinkTrack track)
        {
            await conn.PlayAsync(track);

            int retryCount = 0;
            const int maxRetries = 5;  // Reduced retries
            const int initialDelay = 2000;  // 2 seconds delay
            bool success = false;

            while (!success && retryCount < maxRetries)
            {
                try
                {
                    var youtubeClient = new YoutubeClient();
                    Console.WriteLine($"Attempting to fetch video details for {track.Identifier}, attempt {retryCount + 1}");
                    var video = await youtubeClient.Videos.GetAsync(track.Identifier);
                    Console.WriteLine($"Video details retrieved: {video.Title}");

                    var thumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url;

                    var nowPlayingEmbed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Green,
                        Title = "🎶 Now Playing",
                        Description = $"**🎵 Now Playing:** {track.Title} \n**⏱ Duration:** {track.Length.Minutes}:{track.Length.Seconds:D2}",
                        ImageUrl = thumbnailUrl
                    };

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(nowPlayingEmbed));
                    success = true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"Failed to retrieve video details for {track.Identifier}: {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine($"Skipping video {track.Identifier} after {maxRetries} attempts.");
                        break;
                    }
                    else
                    {
                        await Task.Delay(initialDelay * retryCount);  // Exponential backoff
                    }
                }
            }

            if (!success)
            {
                Console.WriteLine($"Moving on to the next track after failing to retrieve video {track.Identifier}.");
                if (_musicQueues.ContainsKey(ctx.Guild.Id) && _musicQueues[ctx.Guild.Id].Count > 0)
                {
                    var nextTrack = _musicQueues[ctx.Guild.Id].Dequeue();
                    await PlayTrack(ctx, conn, nextTrack);
                }
                else
                {
                    await conn.DisconnectAsync();
                }
            }
        }
    }
}
