using System.IO;
using Newtonsoft.Json.Linq;

namespace Infrastructure
{
	public static class DBData
	{
		private static readonly string appsettings = File.ReadAllText("appsettings.json");
		private static JObject o = JObject.Parse(appsettings);

		private static string server = o["dbServer"].ToString();
		private static string user = o["dbUser"].ToString();
		private static string password = o["dbPassword"].ToString();
		private static string name = o["dbName"].ToString();
		private static string port = o["dbPort"].ToString();
		private static string connectTimeout = o["dbConnectTimeout"].ToString();

		public static string Server
		{
			get { return server; }
		}

		public static string User
		{
			get { return user; }
		}

		public static string Password
		{
			get { return password; }
		}

		public static string Name
		{
			get { return name; }
		}
		public static string Port
		{
			get { return port; }
		}
		public static string ConnectionTimeout
		{
			get { return connectTimeout; }
		}
	}
}
