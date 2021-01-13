using System.IO;
using System.Threading.Tasks;
using Bot.Services;
using Bot.Utilities;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victoria;

namespace Bot
{
	internal static class Program
	{
		private static async Task Main()
		{
			var builder = new HostBuilder()
				.ConfigureAppConfiguration(x =>
				{
					var configuration = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile("appsettings.json", false, true)
						.Build();

					x.AddConfiguration(configuration);
				})
				.ConfigureLogging(x =>
				{
					x.AddConsole();
					x.SetMinimumLevel(LogLevel.Debug);
				})
				.ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
				{
					config.SocketConfig = new DiscordSocketConfig
					{
						LogLevel = LogSeverity.Verbose,
						AlwaysDownloadUsers = true,
						MessageCacheSize = 200
					};

					config.Token = context.Configuration["token"];
				})
				.UseCommandService((context, config) =>
				{
					config = new CommandServiceConfig()
					{
						CaseSensitiveCommands = false,
						LogLevel = LogSeverity.Verbose,
						DefaultRunMode = RunMode.Async,
						ThrowOnError = false
					};
				})
				.ConfigureServices((context, services) =>
				{
					services
					.AddHostedService<CommandHandler>()
					.AddHostedService<MusicHandler>()
					.AddDbContext<Context>()
					.AddLavaNode(x =>
					{
						x.SelfDeaf = true;
						x.Hostname = "127.0.0.1";
						x.Port = 2333;
						x.Authorization = context.Configuration["lavaPassword"];
					})
					.AddSingleton<Servers>()
					.AddSingleton<ServerHelper>()
					.AddSingleton<GoogleHelper>();
				})
				.UseConsoleLifetime();

			var host = builder.Build();
			using (host)
			{
				await host.RunAsync();
			}
		}
	}
}
