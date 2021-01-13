using System;
using System.Threading.Tasks;
using Bot.Utilities;
using Discord;
using Discord.Commands;
using LanguageExt.ClassInstances;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RiotSharp;
using RiotSharp.Misc;

namespace Bot.Modules
{
	public class LeagueOfLegends : ModuleBase<SocketCommandContext>
	{

		private readonly ILogger<LeagueOfLegends> _logger;
		private readonly ServerHelper _serverHelper;

		public LeagueOfLegends(ILogger<LeagueOfLegends> logger, ServerHelper serverHelper)
		{
			_logger = logger;
			_serverHelper = serverHelper;
		}

		[Command("accinfo")]
		public async Task AccInfo(string input = null)
		{
			if (input == null)
			{
				await ReplyAsync("Please enter a name for account information");
				return;
			}

			var Configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json").Build();
			string riotAPI = Configuration["riotAPI"];
			if (string.IsNullOrEmpty(riotAPI))
			{
				Console.WriteLine("No RiotAPI token in appsettings.json found, please add the token");
				return;
			}

			var api = RiotApi.GetDevelopmentInstance(riotAPI);
			try
			{
				var summoner = api.Summoner.GetSummonerByNameAsync(Region.Euw, input).Result;
				var name = summoner.Name;
				var region = summoner.Region;
				var level = summoner.Level;
				var accountId = summoner.AccountId;
				var revisionDate = summoner.RevisionDate;

				var builder = new EmbedBuilder()
				.WithTitle((name, "#", region.ToString()).Concat<TString, string>())
				.AddField("RevisionDate", revisionDate, false)
				.AddField("Level", level, false);

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
