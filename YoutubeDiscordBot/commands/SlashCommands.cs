using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using YoutubeExplode;
using YoutubeExplode.Common;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using DSharpPlus.Net;

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
                // Reconnect Lavalink if the connection was lost
                Console.WriteLine("not connected");
                var endpoint = new ConnectionEndpoint
                {
                    Hostname = "lavalink_v3.muzykant.xyz",
                    Port = 443,
                    Secured = true,
                };

                var lavalinkConfig = new LavalinkConfiguration
                {
                    Password = "https://discord.gg/v6sdrD9kPh",
                    RestEndpoint = endpoint,
                    SocketEndpoint = endpoint
                };

                try
                {
                    await lavalinkInstance.ConnectAsync(lavalinkConfig);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to reconnect to Lavalink: {ex.Message}"));
                    return;
                }
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
            try
            {
                if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No matches found for: {search}"));
                    return;
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("LavaLink Connection Down or Not Established."));
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
            Console.WriteLine($"Attempting to play track: {track.Title}");
            Console.WriteLine($"Track Info: Title - {track.Title}, Uri - {track.Uri}, Duration - {track.Length}");
            await conn.PlayAsync(track);
            Console.WriteLine("Track play request sent.");

            try
            {
                var youtubeClient = new YoutubeClient();
                Console.WriteLine($"Fetching video details for {track.Identifier}");

                var video = await youtubeClient.Videos.GetAsync(track.Identifier);
                Console.WriteLine($"Video details fetched: {video.Title}");

                var thumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url;
                Console.WriteLine($"Thumbnail URL: {thumbnailUrl}");


                string musicDescription = $"**🎵 Banger Playing:** {track.Title} \n" +
                                          $"**⏱ Duration:** {track.Length.Minutes}:{track.Length.Seconds:D2} \n" +
                                          $"**🔗 URL for Kane to use in a YouTube edit:**\n({track.Uri})";

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

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(nowPlayingEmbed));
                Console.WriteLine($"Embed for track {track.Title} sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve video details or send embed for track {track.Identifier}: {ex.Message}");
                var footerEmbed = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ctx.Member.DisplayName}'s song",
                    IconUrl = ctx.User.AvatarUrl
                };
                string musicDescription = $"**🎵 Banger Playing:** {track.Title} \n" +
                          $"**⏱ Duration:** {track.Length.Minutes}:{track.Length.Seconds:D2} \n" +
                          $"**🔗 URL for Kane to use in a YouTube edit:**\n({track.Uri})";
                var playingEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Green,
                    Title = $"🎶 Enjoy your music... You filthy animal 🎶 \n",
                    Description = musicDescription,
                    Footer = footerEmbed
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingEmbed));
                Console.WriteLine($"Default embed sent for track {track.Title} due to exception.");

            }

            // Wait for the track to finish
            while (conn.CurrentState.CurrentTrack != null)
            {
                await Task.Delay(1000);  // Check every second if the track is still playing
            }

            // When the track finishes, play the next one in the queue if available
            if (_musicQueues.ContainsKey(ctx.Guild.Id) && _musicQueues[ctx.Guild.Id].Count > 0)
            {
                var nextTrack = _musicQueues[ctx.Guild.Id].Dequeue();
                await PlayTrack(ctx, conn, nextTrack);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(5));

                if (conn.CurrentState.CurrentTrack == null)
                {
                    // If still no track is playing, disconnect from the voice channel
                    await conn.DisconnectAsync();
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No more tracks in the queue. The bot has disconnected after 5 minutes of inactivity."));
                }
                else
                {
                    // If a track was added to the queue during the delay, continue playing
                    await PlayTrack(ctx, conn, _musicQueues[ctx.Guild.Id].Dequeue());
                }
            }
        }
    }
}
