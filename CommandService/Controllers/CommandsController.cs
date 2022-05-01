using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers
{
	[Route("api/c/platforms/{platformId}/[controller]")]
	[ApiController]
	public class CommandsController : ControllerBase
	{
		private readonly ICommandRepository _commandRepository;
		private readonly IMapper _mapper;

		public CommandsController(ICommandRepository commandRepository, IMapper mapper)
		{
			_commandRepository = commandRepository;
			_mapper = mapper;
		}

		[HttpGet]
		public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
		{
			Console.WriteLine($"--> Hit GetCommandsForPlatform: {platformId}");

			var doesPlatformExist = _commandRepository.PlatformExists(platformId);
			if (!doesPlatformExist)
			{
				return NotFound();
			}

			var commands = _commandRepository.GetCommandsForPlatform(platformId);
			var dtos = _mapper.Map<IEnumerable<CommandReadDto>>(commands);

			return Ok(dtos);
		}

		[HttpGet("{commandId}", Name = "GetCommandForPlatform")]
		public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
		{
			Console.WriteLine($"--> Hit GetCommandForPlatform: {platformId} / {commandId}");

			var doesPlatformExist = _commandRepository.PlatformExists(platformId);
			if (!doesPlatformExist)
			{
				return NotFound();
			}

			var command = _commandRepository.GetCommand(commandId);

			if (command is null)
			{
				return NotFound();
			}

			var dtos = _mapper.Map<CommandReadDto>(command);

			return Ok(dtos);
		}

		[HttpPost]
		public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto dto)
		{
			Console.WriteLine($"--> Hit CreateCommandForPlatform: {platformId}");

			var doesPlatformExist = _commandRepository.PlatformExists(platformId);
			if (!doesPlatformExist)
			{
				return NotFound();
			}

			var command = _mapper.Map<Command>(dto);
			command.PlatformId = platformId;

			_commandRepository.CreateCommand(command);
			_commandRepository.SaveChanges();

			var commandReadDto = _mapper.Map<CommandReadDto>(command);

			return CreatedAtRoute(nameof(GetCommandForPlatform), new
			{
				platformId,
				commandId = commandReadDto.Id,
			}, commandReadDto);
		}
	}
}