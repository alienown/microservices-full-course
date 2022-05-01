using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyndDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PlatformsController : ControllerBase
	{
		private readonly IPlatformRepository _platformRepository;
		private readonly IMapper _mapper;
		private readonly ICommandDataClient _commandDataClient;
		private readonly IMessageBusClient _messageBusClient;

		public PlatformsController(
			IPlatformRepository platformRepository,
			IMapper mapper,
			ICommandDataClient commandDataClient,
			IMessageBusClient messageBusClient)
		{
			_platformRepository = platformRepository;
			_mapper = mapper;
			_commandDataClient = commandDataClient;
			_messageBusClient = messageBusClient;
		}

		[HttpGet]
		public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
		{
			var platforms = _platformRepository.GetAllPlatforms();
			return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
		}

		[HttpGet("{id}", Name = "GetPlatformById")]
		public ActionResult<PlatformReadDto> GetPlatformById(int id)
		{
			var platform = _platformRepository.GetPlatformById(id);

			if (platform != null)
			{
				return Ok(_mapper.Map<PlatformReadDto>(platform));
			}

			return NotFound();
		}

		[HttpPost]
		public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
		{
			var platformModel = _mapper.Map<Platform>(platformCreateDto);
			_platformRepository.CreatePlatform(platformModel);
			_platformRepository.SaveChanges();

			var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);

			// Send sync msg
			try
			{
				await _commandDataClient.SendPlatformToCommand(platformReadDto);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
			}

			// Send async msg
			try
			{
				var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
				platformPublishedDto.Event = "Platform_Published";
				_messageBusClient.PublishNewPlatform(platformPublishedDto);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
			}

			return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
		}
	}
}