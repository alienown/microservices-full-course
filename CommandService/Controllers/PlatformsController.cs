using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers
{
	[Route("api/c/[controller]")]
	[ApiController]
	public class PlatformsController : ControllerBase
	{
		private readonly ICommandRepository _commandRepository;
		private readonly IMapper _mapper;

		public PlatformsController(ICommandRepository commandRepository, IMapper mapper)
		{
			_commandRepository = commandRepository;
			_mapper = mapper;
		}

		[HttpGet]
		public ActionResult<IEnumerable<PlatformReadDto>> GetAllPlatforms()
		{
			Console.WriteLine("--> Getting Platforms from CommandsService");

			var platforms = _commandRepository.GetAllPlatforms();
			var dtos = _mapper.Map<IEnumerable<PlatformReadDto>>(platforms);

			return Ok(dtos);
		}

		[HttpPost]
		public ActionResult TestInboundConnection()
		{
			Console.WriteLine("--> Inbound POST # Command Service");

			return Ok("Inbound test from Platforms Controller");
		}
	}
}