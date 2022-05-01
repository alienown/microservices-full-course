using PlatformService.Dtos;

namespace PlatformService.AsyndDataServices
{
	public interface IMessageBusClient
	{
		void PublishNewPlatform(PlatformPublishedDto platform);
	}
}