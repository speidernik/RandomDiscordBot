using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
	public class Servers
	{
		private readonly Context _context;

		public Servers(Context context)
		{
			_context = context;
		}
		public async Task<string> GetGuildPrefix(ulong id)
		{
			var prefix = await _context.Servers
				.Where(x => x.Id == id)
				.Select(x => x.Prefix)
				.FirstOrDefaultAsync();

			return await Task.FromResult(prefix);
		}

		public async Task ModifyGuildPrefix(ulong id, string prefix)
		{
			var server = await _context.Servers
				.FindAsync(id);

			if (server == null)
			{
				_context.Add(new Server { Id = id, Prefix = prefix });
			}
			else
			{
				server.Prefix = prefix;
			}

			await _context.SaveChangesAsync();
		}

		public async Task<DateTime> GetGuildVCTimestamp(ulong id)
		{
			var timestamp = await _context.Servers
				.Where(x => x.Id == id)
				.Select(x => x.VoiceChannelCooldown)
				.FirstOrDefaultAsync();

			return await Task.FromResult(timestamp);
		}

		public async Task ModifyGuildVCTimeStamp(ulong id, DateTime voiceChannelCooldown)
		{
			var server = await _context.Servers
				.FindAsync(id);

			if (server == null)
			{
				_context.Add(new Server { Id = id, VoiceChannelCooldown = voiceChannelCooldown });
			}
			else
			{
				server.VoiceChannelCooldown = voiceChannelCooldown;
			}

			await _context.SaveChangesAsync();
		}

		public async Task<DateTime> GetGuildTCTimestamp(ulong id)
		{
			var timestamp = await _context.Servers
				.Where(x => x.Id == id)
				.Select(x => x.TextChannelCooldown)
				.FirstOrDefaultAsync();

			return await Task.FromResult(timestamp);
		}

		public async Task ModifyGuildTCTimeStamp(ulong id, DateTime textChannelCooldown)
		{
			var server = await _context.Servers
				.FindAsync(id);

			if (server == null)
			{
				_context.Add(new Server { Id = id, TextChannelCooldown = textChannelCooldown });
			}
			else
			{
				server.TextChannelCooldown = textChannelCooldown;
			}

			await _context.SaveChangesAsync();
		}
	}
}
