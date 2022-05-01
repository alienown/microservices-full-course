using Microsoft.EntityFrameworkCore;
using CommandService.Models;
using CommandService.SyncDataServices.Grpc;

namespace CommandService.Data
{
	public static class PrepDb
	{
		public static void PrepPopulation(IApplicationBuilder app)
		{
			using (var serviceScope = app.ApplicationServices.CreateScope())
			{
				var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

				var commandRepository = serviceScope.ServiceProvider.GetService<ICommandRepository>();

				var platforms = grpcClient.ReturnAllPlatforms();

				SeedData(commandRepository, platforms);
			}
		}

		private static void SeedData(ICommandRepository commandRepository, IEnumerable<Platform> platforms)
		{
			Console.WriteLine("Seeding new platforms...");

			foreach (var platform in platforms)
			{
				var platformExists = commandRepository.ExternalPlatformExists(platform.ExternalId);

				if (!platformExists)
				{
					commandRepository.CreatePlatform(platform);
				}
			}

			commandRepository.SaveChanges();
		}
	}
}