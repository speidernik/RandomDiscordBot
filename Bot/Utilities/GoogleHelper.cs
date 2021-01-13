using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;

namespace Bot.Utilities
{
	public class GoogleHelper
	{
		public static string ApplicationName = "CalendarConsole";

		public List<string> ShowUpCommingEvent()
		{
			ServiceAccountCredential serviceCredential = GetServiceCredential();

			// Creat Google Calendar API service.
			CalendarService calenderService = GetCalenderService(serviceCredential);

			// Define parameters of request
			EventsResource.ListRequest calenderRequest = calenderService.Events.List("primary");
			calenderRequest.TimeMin = DateTime.Now;
			calenderRequest.ShowDeleted = false;
			calenderRequest.SingleEvents = true;
			calenderRequest.MaxResults = 10;
			calenderRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

			// List events.
			Events events = calenderRequest.Execute();
			List<string> results = new List<string>();
			if (events.Items != null && events.Items.Count > 0)
			{
				foreach (var eventItem in events.Items)
				{
					string when = eventItem.Start.DateTime.ToString();
					if (string.IsNullOrEmpty(when))
					{
						when = eventItem.Start.Date;
					}
					results.Add(string.Format("{0} ({1})", eventItem.Summary, when));
				}
				return results;
			}
			else
			{
				return null;
			}
		}

		private CalendarService GetCalenderService(ServiceAccountCredential serviceCredential)
		{

			// Creat Google Calendar API service.
			var calenderService = new CalendarService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = serviceCredential,
				ApplicationName = ApplicationName,
			});

			return calenderService;
		}

		private ServiceAccountCredential GetServiceCredential()
		{
			string[] Scopes = { CalendarService.Scope.Calendar };
			var serviceAccountJson = File.ReadAllText("credentials.json");
			var o = JObject.Parse(serviceAccountJson);

			var email = o["client_email"].ToString();
			var privateKey = o["private_key"].ToString();

			ServiceAccountCredential serviceCredential = new ServiceAccountCredential(
				new ServiceAccountCredential.Initializer(email)
				{
					Scopes = new[] { CalendarService.Scope.CalendarReadonly }
				}.FromPrivateKey(privateKey));

			return serviceCredential;
		}
	}
}
