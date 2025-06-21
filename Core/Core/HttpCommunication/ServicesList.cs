using Core.HttpCommunication.Exception;
using Core.HttpCommunication.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Core.HttpCommunication
{
	public static class ServicesList
	{
		private static readonly Dictionary<string, string> _services = new();
		public static string TryGetService(string serviceName)
		{
			var succeed = _services.TryGetValue(serviceName, out var service);
			if(!succeed || service == null) throw new ServiceNotFoundException(serviceName);
			return service;
		}
		public static bool TryAddService(string servicesJson)
		{
			var services = JsonConvert.DeserializeObject<IEnumerable<ServiceModel>>(servicesJson);
			if(services == null) return false;
			foreach(var service in services)
				_services.Add(service.Name, service.Name);
			return true;
		}
		public static bool TryAddService(IConfigurationSection section)
		{
			var services = section.Get<List<ServiceModel>>();
			if (services == null) return false;
			foreach (var service in services)
				_services.Add(service.Name, service.Uri);
			return true;
		}
	}
}
