using System.Text;
using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandService.AsyncDataServices
{
	public class MessageBusSubscriber : BackgroundService
	{
		private readonly IConfiguration _configuration;
		private readonly IEventProcessor _eventProcessor;
		private IConnection _connection;
		private IModel _channel;
		private string _queueName;

		public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
		{
			_configuration = configuration;
			_eventProcessor = eventProcessor;

			InitializeRabbitMQ();
		}

		private void InitializeRabbitMQ()
		{
			var connFactory = new ConnectionFactory()
			{
				HostName = _configuration["RabbitMQHost"],
				Port = _configuration.GetValue<int>("RabbitMQPort"),
			};

			_connection = connFactory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
			_queueName = _channel.QueueDeclare().QueueName;
			_channel.QueueBind(queue: _queueName, exchange: "trigger", routingKey: string.Empty);

			Console.WriteLine("--> Listening on the Message Bus...");

			_connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
		}

		private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
		{
			Console.WriteLine("--> Connection Shutdown");
		}

		public override void Dispose()
		{
			if (_channel.IsOpen)
			{
				_channel.Close();
				_connection.Close();
			}

			base.Dispose();
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			stoppingToken.ThrowIfCancellationRequested();

			var consumer = new EventingBasicConsumer(_channel);

			consumer.Received += (ModuleHandle, ea) =>
			{
				Console.WriteLine("--> Event Received");

				var body = ea.Body;

				var message = Encoding.UTF8.GetString(body.ToArray());

				_eventProcessor.ProcessEvent(message);
			};

			_channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

			return Task.CompletedTask;
		}
	}
}