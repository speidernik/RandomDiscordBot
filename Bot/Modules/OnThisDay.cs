using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bot.Utilities;
using Discord;
using Discord.Commands;
using LanguageExt.ClassInstances;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Bot.Modules
{
	public class OnThisDay : ModuleBase<SocketCommandContext>
	{
		private readonly ILogger<OnThisDay> _logger;
		private readonly ServerHelper _serverHelper;

		public OnThisDay(ILogger<OnThisDay> logger, ServerHelper serverHelper)
		{
			_logger = logger;
			_serverHelper = serverHelper;
		}

		[Command("event")]
		public async Task Event(string sDate = null)
		{
			List<string> data = await _serverHelper.CheckDateValid(sDate);
			if (data.Count == 1)
			{
				await ReplyAsync(data[0]);
				return;
			}

			var client = new HttpClient();
			var result = await client.GetStringAsync(string.Format("https://byabbe.se/on-this-day/{0}/{1}/events.json", data[1], data[0]));
			JObject json = JObject.Parse(result);
			JArray eventsItems = (JArray)json["events"];

			int eventsLength = eventsItems.Count;
			Random rnd = new Random();
			int element = rnd.Next(0, eventsLength - 1);

			JArray wikipediaItems = (JArray)json["events"][element]["wikipedia"];
			int wikipediaLength = wikipediaItems.Count;

			var builder = new EmbedBuilder()
				.WithColor(new Color(33, 176, 252))
				.AddField("Date", ((string)json["date"], " ", (string)json["events"][element]["year"]).Concat<TString, string>(), true)
				.AddField("Description", (string)json["events"][element]["description"], true);
			for (int i = 0; i < wikipediaLength; i++)
			{
				builder.AddField((string)json["events"][element]["wikipedia"][i]["title"], (string)json["events"][element]["wikipedia"][i]["wikipedia"], false);
			}
			var embed = builder.Build();

			await Context.Channel.SendMessageAsync(null, false, embed);
		}

		[Command("death")]
		public async Task Death(string sDate = null)
		{
			List<string> data = await _serverHelper.CheckDateValid(sDate);
			if (data.Count == 1)
			{
				await ReplyAsync(data[0]);
				return;
			}

			var client = new HttpClient();
			var result = await client.GetStringAsync(string.Format("https://byabbe.se/on-this-day/{0}/{1}/deaths.json", data[1], data[0]));
			JObject json = JObject.Parse(result);

			JArray deathsItems = (JArray)json["deaths"];
			int deathsLength = deathsItems.Count;
			Random rnd = new Random();
			int death = rnd.Next(0, deathsLength - 1);

			JArray wikipediaItems = (JArray)json["deaths"][death]["wikipedia"];
			int wikipediaLength = wikipediaItems.Count;

			var builder = new EmbedBuilder()
				.WithColor(new Color(33, 176, 252))
				.WithTitle((string)json["deaths"][death]["wikipedia"][0]["title"])
				//.WithUrl((string)json["deaths"][death]["wikipedia"][0]["wikipedia"])
				.AddField("Date", ((string)json["date"], " ", (string)json["deaths"][death]["year"]).Concat<TString, string>(), true)
				.AddField("Description", (string)json["deaths"][death]["description"], true);
			for (int i = 0; i < wikipediaLength; i++)
			{
				builder.AddField((string)json["deaths"][death]["wikipedia"][i]["title"], (string)json["deaths"][death]["wikipedia"][i]["wikipedia"], false);
			}
			var embed = builder.Build();

			await Context.Channel.SendMessageAsync(null, false, embed);
		}

		[Command("birth")]
		public async Task Birth(string sDate = null)
		{
			List<string> data = await _serverHelper.CheckDateValid(sDate);
			if (data.Count == 1)
			{
				await ReplyAsync(data[0]);
				return;
			}

			var client = new HttpClient();
			var result = await client.GetStringAsync(string.Format("https://byabbe.se/on-this-day/{0}/{1}/births.json", data[1], data[0]));
			JObject json = JObject.Parse(result);
			JArray birthsItems = (JArray)json["births"];

			int birthsLength = birthsItems.Count;
			Random rnd = new Random();
			int birth = rnd.Next(0, birthsLength - 1);

			JArray wikipediaItems = (JArray)json["births"][birth]["wikipedia"];
			int wikipediaLength = wikipediaItems.Count;

			var builder = new EmbedBuilder()
				.WithColor(new Color(33, 176, 252))
				.WithTitle((string)json["births"][birth]["wikipedia"][0]["title"])
				//.WithUrl((string)json["births"][birth]["wikipedia"][0]["wikipedia"])
				.AddField("Date", ((string)json["date"], " ", (string)json["births"][birth]["year"]).Concat<TString, string>(), true)
				.AddField("Description", (string)json["births"][birth]["description"], true);
			for (int i = 0; i < wikipediaLength; i++)
			{
				builder.AddField((string)json["births"][birth]["wikipedia"][i]["title"], (string)json["births"][birth]["wikipedia"][i]["wikipedia"], false);
			}
			var embed = builder.Build();

			await Context.Channel.SendMessageAsync(null, false, embed);
		}
	}
}
