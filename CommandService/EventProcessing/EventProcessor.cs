using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;

namespace CommandService.EventProcessing
{
	public class EventProcessor : IEventProcessor
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IMapper _mapper;

		public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
		{
			_scopeFactory = scopeFactory;
			_mapper = mapper;
		}

		public void ProcessEvent(string message)
		{
			var eventType = DetermineEvent(message);

			switch (eventType)
			{
				case EventType.PlatformPublished:
					AddPlatform(message);
					break;
				default:
					break;
			}

			throw new NotImplementedException();
		}

		private EventType DetermineEvent(string message)
		{
			Console.WriteLine("--> Determining Event");

			var eventType = JsonSerializer.Deserialize<GenericEventDto>(message);

			switch (eventType?.Event)
			{
				case "Platform_Published":
					Console.WriteLine("--> Platform Published Event Detected");
					return EventType.PlatformPublished;
				default:
					Console.WriteLine("--> Could not determine event type");
					return EventType.Undetermined;
			}
		}

		private void AddPlatform(string platformPublishedMessage)
		{
			var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
			var platform = _mapper.Map<Platform>(platformPublishedDto);

			using (var scope = _scopeFactory.CreateScope())
			{
				var repo = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

				try
				{
					var platformExists = repo.ExternalPlatformExists(platform.ExternalId);
					if (!platformExists)
					{
						repo.CreatePlatform(platform);
						repo.SaveChanges();
						Console.WriteLine("--> Platform added!");
					}
					else
					{
						Console.WriteLine("--> Platform already exists...");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"--> Could not add Platform to DB {ex.Message}");
				}
			}
		}
	}

	enum EventType
	{
		PlatformPublished,
		Undetermined
	}
}