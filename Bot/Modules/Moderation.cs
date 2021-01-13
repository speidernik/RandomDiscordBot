using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Logging;


namespace Bot.Modules
{
	public class Moderation : ModuleBase<SocketCommandContext>
	{
		private readonly ILogger<Moderation> _logger;
		private readonly Servers _servers;
		private readonly DiscordSocketClient _client;

		public Moderation(ILogger<Moderation> logger, Servers servers, DiscordSocketClient client)
		{
			_logger = logger;
			_servers = servers;
			_client = client;
		}

		[Command("purge")]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task Purge(int amount)
		{
			var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
			await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

			var message = await Context.Channel.SendMessageAsync(string.Format("{0} messages deleted successfully!", messages.Count()));
			await Task.Delay(2500);
			await message.DeleteAsync();
		}

		[Command("prefix")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task Prefix(string prefix = null)
		{
			if (prefix == null)
			{
				var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";
				await ReplyAsync(string.Format("The current prefix of this bot is `{0}`.", guildPrefix));
				return;
			}

			if (prefix.Length > 8)
			{
				await ReplyAsync("The lenght of the new prfix is too long!");
				return;
			}

			await _client.SetGameAsync(string.Format("Prefix is {0}", prefix));
			await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
			await ReplyAsync(string.Format("The prefix has been adjusted to `{0}`", prefix));
		}

		[Command("tc")]
		public async Task TextChannelName([Remainder] string name = null)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				await ReplyAsync("Enter a new name for this channel");
				return;
			}
			DateTime timeDB = _servers.GetGuildTCTimestamp(Context.Guild.Id).Result;
			DateTime timeNow = DateTime.Now;
			if ((timeNow - timeDB).TotalMinutes <= 5)
			{
				await ReplyAsync(string.Format("Text channel can be renamed again at: {0}", timeDB.AddMinutes(5).ToString("HH:mm:ss")));
				return;
			}
			await _servers.ModifyGuildTCTimeStamp(Context.Guild.Id, timeNow);

			var channel = Context.Channel as ITextChannel;
			await channel.ModifyAsync(x =>
			{
				x.Name = name;
			});

			await ReplyAsync(string.Format("Text channel name changed from `{0}` to `{1}`", channel, name));
		}

		[Command("vc")]
		public async Task VoiceChannelName([Remainder] string name = null)
		{
			var voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				await ReplyAsync("Enter a new name for your voice channel");
				return;
			}

			DateTime timeDB = _servers.GetGuildVCTimestamp(Context.Guild.Id).Result;
			DateTime timeNow = DateTime.Now;
			if ((timeNow - timeDB).TotalMinutes <= 5)
			{
				await ReplyAsync(string.Format("Voice channel can be renamed at: {0}", timeDB.AddMinutes(5).ToString("HH:mm:ss")));
				return;
			}
			await _servers.ModifyGuildVCTimeStamp(Context.Guild.Id, timeNow);

			string oldName = voiceState.VoiceChannel.Name;
			ulong id = voiceState.VoiceChannel.Id;
			var channel = _client.GetChannel(id) as IVoiceChannel;
			if (channel == null)
			{
				return;
			}

			try
			{
				await channel.ModifyAsync(x =>
				{
					x.Name = name;
				});
			}
			catch (Exception ex)
			{
				await ReplyAsync(ex.Message);
			}

			await ReplyAsync(string.Format("Voice channel name changed from `{0}` to `{1}`", oldName, name));
		}
	}
}
