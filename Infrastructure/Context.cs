using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
	public class Context : DbContext
	{
		public DbSet<Server> Servers { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseMySql(string.Format("server={0};user={1};password={2};database={3};port={4};Connect Timeout={5};",
				DBData.Server, DBData.User, DBData.Password, DBData.Name, DBData.Port, DBData.ConnectionTimeout));
	}

	public class Server
	{
		public ulong Id { get; set; }
		public string Prefix { get; set; }
		public DateTime VoiceChannelCooldown { get; set; }
		public DateTime TextChannelCooldown { get; set; }
	}
}
