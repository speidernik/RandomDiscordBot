using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Bot.Modules
{
	public class Fun : ModuleBase<SocketCommandContext>
	{
		private readonly ILogger<Fun> _logger;

		public Fun(ILogger<Fun> logger)
			=> _logger = logger;

		[Command("meme")]
		[Alias("reddit")]
		public async Task Meme(string subreddit = null)
		{
			var client = new HttpClient();
			var result = await client.GetStringAsync(string.Format("https://reddit.com/r/{0}/random.json?limit=1", subreddit ?? "memes"));
			if (!result.StartsWith("["))
			{
				await Context.Channel.SendMessageAsync("This subreddit doesn't exist");
				return;
			}

			JArray arr = JArray.Parse(result);
			JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

			var builder = new EmbedBuilder()
				.WithImageUrl(post["url"].ToString())
				.WithColor(new Color(33, 176, 252))
				.WithTitle(post["title"].ToString())
				.WithUrl(string.Format("{0}{1}", "https://reddit.com", post["permalink"].ToString()))
				.WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}");
			var embed = builder.Build();
			await Context.Channel.SendMessageAsync(null, false, embed);
		}

		[Command("map")]
		public async Task Map()
		{
			var builder = new EmbedBuilder()
				.WithColor(new Color(33, 176, 252))
				.WithTitle("Minecraft Server Map")
				.WithUrl("http://85.214.203.232:8100/");
			var embed = builder.Build();
			await Context.Channel.SendMessageAsync(null, false, embed);
		}

		[Command("ip")]
		public async Task Ip()
		{
			var builder = new EmbedBuilder()
				.WithColor(new Color(33, 176, 252))
				.WithTitle("85.214.203.232");
			var embed = builder.Build();
			await Context.Channel.SendMessageAsync(null, false, embed);
		}
	}
}
