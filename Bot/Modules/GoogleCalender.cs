using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Utilities;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Modules
{
	public class GoogleCalender : ModuleBase<SocketCommandContext>
	{
		private readonly ILogger<GoogleCalender> _logger;
		private readonly GoogleHelper _googleHelper;
		public static string credentialsPath = "credentials.json";

		public GoogleCalender(ILogger<GoogleCalender> logger, GoogleHelper googleHelper)
		{
			_logger = logger;
			_googleHelper = googleHelper;
		}

		[Command("getevents")]
		public async Task GetEvents()
		{
			try
			{
				List<string> eventResults = new List<string>();
				eventResults = _googleHelper.ShowUpCommingEvent();
				//Console.WriteLine(eventResults.ToString());



				var builder = new EmbedBuilder()
				.WithTitle("Upcomming events")
				.WithColor(new Color(33, 176, 252));
				foreach (var eventResult in eventResults)
				{
					builder.AddField("Event", eventResult, false);
					Console.WriteLine(eventResult);
				}
				var embed = builder.Build();

				await Context.Channel.SendMessageAsync(null, false, embed);
				return;
			}
			catch (Exception ex)
			{
				await ReplyAsync(ex.Message);
				return;
			}
		}
	}
}
