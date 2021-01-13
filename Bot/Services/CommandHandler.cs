using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bot.Common;
using Bot.Utilities;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Victoria;

namespace Bot.Services
{
	public class CommandHandler : InitializedService
	{
		private readonly IServiceProvider _provider;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _service;
		private readonly IConfiguration _config;
		private readonly Servers _servers;
		private readonly ServerHelper _serverHelper;
		private readonly LavaNode _lavaNode;

		public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, ServerHelper serverHelper, LavaNode lavaNode)
		{
			_provider = provider;
			_client = client;
			_service = service;
			_config = config;
			_servers = servers;
			_serverHelper = serverHelper;
			_lavaNode = lavaNode;
		}

		public override async Task InitializeAsync(CancellationToken cancellationToken)
		{
			_client.MessageReceived += OnMessageReceived;

			_service.CommandExecuted += OnCommandExecuted;
			await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
		}

		private async Task OnMessageReceived(SocketMessage arg)
		{
			if (!(arg is SocketUserMessage message))
			{
				return;
			}

			if (message.Source != MessageSource.User)
			{
				return;
			}

			var argPos = 0;
			var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!";
			if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
			{
				return;
			}

			var context = new SocketCommandContext(_client, message);
			await _service.ExecuteAsync(context, argPos, _provider);
		}

		private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
		{
			if (command.IsSpecified && !result.IsSuccess)
			{
				await (context.Channel as ISocketMessageChannel).SendErrorAsync("Error", result.ErrorReason);
			}
		}
	}
}
