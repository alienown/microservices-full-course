using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyndDataServices
{
	public class MessageBusClient : IMessageBusClient
	{
		private readonly IConfiguration _configuration;
		private readonly IConnection _connection;
		private readonly IModel _channel;

		public MessageBusClient(IConfiguration configuration)
		{
			_configuration = configuration;

			var connFactory = new ConnectionFactory
			{
				HostName = _configuration["RabbitMQHost"],
				Port = _configuration.GetValue<int>("RabbitMQPort"),
			};

			try
			{
				_connection = connFactory.CreateConnection();
				_channel = _connection.CreateModel();
				_channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
				_connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
				Console.WriteLine("--> Connected to MessageBus");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
			}
		}

		private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
		{
			Console.WriteLine("--> RabbitMQ Connection Shutdown");
		}

		public void PublishNewPlatform(PlatformPublishedDto platform)
		{
			var message = JsonSerializer.Serialize(platform);

			if (_connection.IsOpen)
			{
				Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
				SendMessage(message);
			}
			else
			{
				Console.WriteLine("--> RabbitMQ connection closed, not sending");
			}
		}

		public void Dispose()
		{
			Console.WriteLine("MessageBus Disposed");

			if (_channel.IsOpen)
			{
				_channel.Close();
				_connection.Close();
			}
		}

		private void SendMessage(string message)
		{
			var body = Encoding.UTF8.GetBytes(message);

			_channel.BasicPublish(exchange: "trigger",
				routingKey: string.Empty,
				basicProperties: null,
				body: body);

			Console.WriteLine($"--> We have sent {message}");
		}
	}
}