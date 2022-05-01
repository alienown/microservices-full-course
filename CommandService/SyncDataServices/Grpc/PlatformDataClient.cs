using AutoMapper;
using CommandService.Models;
using Grpc.Net.Client;
using PlatformService;

namespace CommandService.SyncDataServices.Grpc
{
	public class PlatformDataClient : IPlatformDataClient
	{
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;

		public PlatformDataClient(IConfiguration configuration, IMapper mapper)
		{
			_configuration = configuration;
			_mapper = mapper;
		}

		public IEnumerable<Platform> ReturnAllPlatforms()
		{
			var url = _configuration["GrpcPlatform"];

			Console.WriteLine($"--> Calling GRPC Service {url}");

			var channel = GrpcChannel.ForAddress(url);

			var client = new GrpcPlatform.GrpcPlatformClient(channel);

			var request = new GetAllRequest();

			try
			{
				var response = client.GetAllPlatforms(request);

				var platforms = _mapper.Map<IEnumerable<Platform>>(response.Platform);

				return platforms;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"--> Could not call GRPC Server {ex.Message}");
			}

			return null;
		}
	}
}