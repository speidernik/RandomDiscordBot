using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;

namespace Bot.Utilities
{
	public class ServerHelper
	{
		private readonly Servers _servers;

		public ServerHelper(Servers servers)
		{
			_servers = servers;
		}

		public async Task<List<string>> CheckDateValid(string sDate)
		{
			string sMonth = null;
			string sDay = null;

			if (sDate == null)
			{
				sMonth = DateTime.Now.ToString("MM");
				sDay = DateTime.Now.ToString("dd");
				List<string> today = new List<string> { sDay, sMonth };
				return await Task.FromResult(today);
			}
			else
			{
				if (!sDate.Contains("."))
				{
					List<string> error = new List<string> { "Please enter a valid date. day.month" };
					return await Task.FromResult(error);
				}
				string[] splitDate = sDate.Split(".");
				sDay = splitDate[0];
				sMonth = splitDate[1];

				string checkDate = $"2000-{sMonth}-{sDay}";
				DateTime dDate;

				if (!DateTime.TryParse(checkDate, out dDate))
				{
					List<string> error = new List<string> { "Please enter a valid date. day.month" };
					return await Task.FromResult(error);
				}
			}
			List<string> customDate = new List<string> { sDay, sMonth };
			return await Task.FromResult(customDate);
		}
	}
}
