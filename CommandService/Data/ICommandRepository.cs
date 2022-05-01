using CommandService.Models;

namespace CommandService.Data
{
	public interface ICommandRepository
	{
		bool SaveChanges();
		
		IEnumerable<Platform> GetAllPlatforms();
		void CreatePlatform(Platform platform);
		bool PlatformExists(int platformId);
		bool ExternalPlatformExists(int externalPlatformId);

		IEnumerable<Command> GetCommandsForPlatform(int platformId);
		Command GetCommand(int commandId);
		void CreateCommand(Command command);
	}
}