using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LanguageExt;
using Microsoft.Extensions.Logging;


namespace Bot.Modules
{
	public class General : ModuleBase<SocketCommandContext>
	{
		private readonly ILogger<General> _logger;
		public General(ILogger<General> logger)
			=> _logger = logger;

		[Command("ping")]
		[Summary("Show current latency.")]
		public async Task Ping() => await ReplyAsync($"Latency: {Context.Client.Latency} ms");

		[Command("info")]
		public async Task Info(SocketGuildUser user = null)
		{
			if (user == null)
			{
				var builder = new EmbedBuilder()
					.WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
					.WithDescription("In this message you can see some information about yourself")
					.WithColor(new Color(33, 176, 252))
					.AddField("Username", Context.User.Username, true)
					.AddField("User ID", Context.User.Id, true)
					.AddField("Discriminator", Context.User.Discriminator, true)
					.AddField("Created at", Context.User.CreatedAt.LocalDateTime.ToString("dd/MM/yyyy"), true)
					.AddField("Joined at", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
					.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
					 .WithCurrentTimestamp();
				var embed = builder.Build();
				await Context.Channel.SendMessageAsync(null, false, embed);
			}
			else
			{
				var builder = new EmbedBuilder()
					.WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
					.WithDescription($"In this message you can see some information about {user.Username}")
					.WithColor(new Color(33, 176, 252))
					.AddField("Username", user.Username, true)
					.AddField("User ID", user.Id, true)
					.AddField("Discriminator", user.Discriminator, true)
					.AddField("Created at", user.CreatedAt.LocalDateTime.ToString("dd/MM/yyyy"), true)
					.AddField("Joined at", user.JoinedAt.Value.ToString("dd/MM/yyyy"), true)
					.AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
					 .WithCurrentTimestamp();
				var embed = builder.Build();
				await Context.Channel.SendMessageAsync(null, false, embed);
			}
		}

		[Command("server")]
		public async Task Server()
		{
			var builder = new EmbedBuilder()
				.WithThumbnailUrl(Context.Guild.IconUrl)
				.WithDescription("In this message you can find infos for the current server.")
				.WithTitle($"{Context.Guild.Name} Information")
				.WithColor(new Color(33, 176, 252))
				.AddField("Created at", Context.Guild.CreatedAt.LocalDateTime.ToString("dd/MM/yyyy"))
				.AddField("Membercount", (Context.Guild as SocketGuild).MemberCount + " member", true)
				.AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status == UserStatus.Online).Count() + " members", true);
			var embed = builder.Build();
			await Context.Channel.SendMessageAsync(null, false, embed);
		}
	}
}
