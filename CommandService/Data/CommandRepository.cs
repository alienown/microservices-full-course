using CommandService.Models;

namespace CommandService.Data
{
	public class CommandRepository : ICommandRepository
	{
		private readonly AppDbContext _context;

		public CommandRepository(AppDbContext context)
		{
			_context = context;
		}

		public void CreateCommand(Command command)
		{
			if (command is null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			_context.Commands.Add(command);
		}

		public void CreatePlatform(Platform platform)
		{
			if (platform is null)
			{
				throw new ArgumentNullException(nameof(platform));
			}

			_context.Platforms.Add(platform);
		}

		public IEnumerable<Platform> GetAllPlatforms()
		{
			var platforms = _context.Platforms.ToList();
			return platforms;
		}

		public Command GetCommand(int commandId)
		{
			var command = _context.Commands.FirstOrDefault(x => x.Id == commandId);
			return command;
		}

		public IEnumerable<Command> GetCommandsForPlatform(int platformId)
		{
			var commands = _context.Commands
				.Where(x => x.PlatformId == platformId)
				.ToList();

			return commands;
		}

		public bool PlatformExists(int platformId)
		{
			var exists = _context.Platforms.Any(x => x.Id == platformId);
			return exists;
		}

		public bool ExternalPlatformExists(int externalPlatformId)
		{
			var exists = _context.Platforms.Any(x => x.ExternalId == externalPlatformId);
			return exists;
		}

		public bool SaveChanges()
		{
			var result = _context.SaveChanges() >= 0;
			return result;
		}
	}
}