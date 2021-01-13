using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Modules
{
	public class Minecraft : ModuleBase<SocketCommandContext>
	{
		private readonly ILogger<Minecraft> _logger;

		public Minecraft(ILogger<Minecraft> logger)
			=> _logger = logger;
	}
}
