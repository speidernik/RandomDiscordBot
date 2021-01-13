using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.Enums;

namespace Bot.Modules
{
	public class MusicModule : ModuleBase<SocketCommandContext>
	{
		private readonly LavaNode _lavaNode;
		private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

		public MusicModule(LavaNode lavaNode, ILoggerFactory loggerFactory)
		{
			_lavaNode = lavaNode;
			_disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
		}

		[Command("join", RunMode = RunMode.Async)]
		public async Task JoinAsync()
		{
			if (_lavaNode.HasPlayer(Context.Guild))
			{
				await ReplyAsync("I'm already connected to a voice channel!");
				return;
			}

			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			try
			{
				await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
				await ReplyAsync(string.Format("Joined {0}!", voiceState.VoiceChannel.Name));
			}
			catch (Exception exception)
			{
				await ReplyAsync(exception.Message);
			}
		}

		[Command("leave", RunMode = RunMode.Async)]
		public async Task LeaveAsync()
		{
			if (!_lavaNode.HasPlayer(Context.Guild))
			{
				await ReplyAsync("I'm not connected to a voice channel!");
				return;
			}

			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			try
			{
				await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
				await ReplyAsync(string.Format("Left {0}!", voiceState.VoiceChannel.Name));
			}
			catch (Exception exception)
			{
				await ReplyAsync(exception.Message);
			}
		}

		[Command("play", RunMode = RunMode.Async)]
		public async Task PlayAsync([Remainder] string searchQuery = null)
		{
			if (string.IsNullOrWhiteSpace(searchQuery))
			{
				await ReplyAsync("Please provide search terms.");
				return;
			}

			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			if (!_lavaNode.HasPlayer(Context.Guild))
			{
				try
				{
					await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
					//await ReplyAsync(string.Format("Joined {0}!", voiceState.VoiceChannel.Name));
				}
				catch (Exception exception)
				{
					await ReplyAsync(exception.Message);
				}
			}

			var queries = searchQuery.Split(' ');
			foreach (var query in queries)
			{
				var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
				if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
					searchResponse.LoadStatus == LoadStatus.NoMatches)
				{
					await ReplyAsync(string.Format("I wasn't able to find anything for `{0}`.", query));
					return;
				}

				var player = _lavaNode.GetPlayer(Context.Guild);

				if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
				{
					if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
					{
						foreach (var track in searchResponse.Tracks)
						{
							player.Queue.Enqueue(track);
						}

						await ReplyAsync(string.Format("Enqueued {0} tracks.", searchResponse.Tracks.Count));
					}
					else
					{
						var track = searchResponse.Tracks[0];
						player.Queue.Enqueue(track);
						await ReplyAsync(string.Format("Enqueued: {0}\nDuration: {1}", track.Title, track.Duration));
					}
				}
				else
				{
					var track = searchResponse.Tracks[0];

					if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
					{
						for (var i = 0; i < searchResponse.Tracks.Count; i++)
						{
							if (i == 0)
							{
								await player.PlayAsync(track);

								var builder = new EmbedBuilder()
								.WithColor(new Discord.Color(33, 176, 252))
								.WithTitle(string.Format("{0}", track.Title))
								.WithUrl(track.Url)
								.AddField("Duration", track.Duration, false)
								.AddField("Tracks", searchResponse.Tracks.Count);
								var embed = builder.Build();

								await Context.Channel.SendMessageAsync(null, false, embed);
							}
							else
							{
								player.Queue.Enqueue(searchResponse.Tracks[i]);
							}
						}

						await ReplyAsync(string.Format("Enqueued {0} tracks.", searchResponse.Tracks.Count));
					}
					else
					{
						await player.PlayAsync(track);

						var builder = new EmbedBuilder()
						.WithColor(new Discord.Color(33, 176, 252))
						.WithTitle(string.Format("{0}", track.Title))
						.WithUrl(track.Url)
						.AddField("Duration", track.Duration, false);
						var embed = builder.Build();

						await Context.Channel.SendMessageAsync(null, false, embed);
					}
				}
			}
		}

		[Command("skip", RunMode = RunMode.Async)]
		public async Task SkipAsync()
		{
			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			if (!_lavaNode.HasPlayer(Context.Guild))
			{
				await ReplyAsync("I'm not connected to a voice channel!");
				return;
			}

			var player = _lavaNode.GetPlayer(Context.Guild);
			if (voiceState.VoiceChannel != player.VoiceChannel)
			{
				await ReplyAsync("You need to be in the same voicechannel as me!");
				return;
			}

			if (player.Queue.Count == 0)
			{
				await ReplyAsync("There are no more songs in the queue");
				return;
			}

			await player.SkipAsync();

			var builder = new EmbedBuilder()
					.WithColor(new Discord.Color(33, 176, 252))
					.WithTitle(string.Format("{0}", player.Track.Title))
					.WithUrl(player.Track.Url)
					.AddField("Duration", player.Track.Duration, false);
			var embed = builder.Build();
			await Context.Channel.SendMessageAsync(null, false, embed);
		}

		[Command("pause", RunMode = RunMode.Async)]
		public async Task PauseAsync()
		{
			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			if (!_lavaNode.HasPlayer(Context.Guild))
			{
				await ReplyAsync("I'm not connected to a voice channel!");
				return;
			}

			var player = _lavaNode.GetPlayer(Context.Guild);
			if (voiceState.VoiceChannel != player.VoiceChannel)
			{
				await ReplyAsync("You need to be in the same voicechannel as me!");
				return;
			}

			if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
			{
				await ReplyAsync("The music is already paused!");
				return;
			}

			await player.PauseAsync();
			await ReplyAsync("Paused the music");
		}

		[Command("resume", RunMode = RunMode.Async)]
		public async Task ResumeAsync()
		{
			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			if (!_lavaNode.HasPlayer(Context.Guild))
			{
				await ReplyAsync("I'm not connected to a voice channel!");
				return;
			}

			var player = _lavaNode.GetPlayer(Context.Guild);
			if (voiceState.VoiceChannel != player.VoiceChannel)
			{
				await ReplyAsync("You need to be in the same voicechannel as me!");
				return;
			}

			if (player.PlayerState == PlayerState.Playing)
			{
				await ReplyAsync("The music is already playing!");
				return;
			}

			await player.ResumeAsync();
			await ReplyAsync("Resumed the track");
		}

		private async Task GetSongAsync(string query)
		{
			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			if (!_lavaNode.HasPlayer(Context.Guild))
			{
				try
				{
					await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
					await ReplyAsync(string.Format("Joined {0}!", voiceState.VoiceChannel.Name));
				}
				catch (Exception exception)
				{
					await ReplyAsync(exception.Message);
				}
			}

			var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
			if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
				searchResponse.LoadStatus == LoadStatus.NoMatches)
			{
				await ReplyAsync(string.Format("I wasn't able to find for `{0}`.", query));
				return;
			}

			var player = _lavaNode.GetPlayer(Context.Guild);
			var track = searchResponse.Tracks[0];

			await player.PlayAsync(track);

			var builder = new EmbedBuilder()
			.WithColor(new Discord.Color(33, 176, 252))
			.WithTitle(string.Format("{0}", track.Title))
			.WithUrl(track.Url)
			.AddField("Duration", track.Duration, false);
			var embed = builder.Build();

			await Context.Channel.SendMessageAsync(null, false, embed);
		}
		[Command("unser", RunMode = RunMode.Async)]
		public async Task UnserAsync()
		{
			string query = "https://www.youtube.com/watch?v=BulFwGSi8bc";
			await GetSongAsync(query);
		}

		[Command("comfy", RunMode = RunMode.Async)]
		public async Task ComfyAsync()
		{
			string query = "Izzie Naylor - BLUE MOON cover but it's Lofi ( J A R I T E N P.H. Remix) | Lofi";
			await GetSongAsync(query);
		}

		[Command("horny", RunMode = RunMode.Async)]
		public async Task HornyAsync()
		{
			string query = "https://www.youtube.com/watch?v=izGwDsrQ1eQ";
			await GetSongAsync(query);
		}
	}
}
