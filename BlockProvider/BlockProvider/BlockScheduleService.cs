using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Logging;

namespace BlockProvider
{
    public class BlockScheduleService:IHostedService
    {
        private const string EXCHANGE_NAME = "e.b.forward";
        private const string ROUTING_KEY = "r.b.app.block";
        
        private readonly ILogger<BlockScheduleService> _logger;
        private readonly ConnectionFactory _connectionFactory;
        private readonly System.Timers.Timer _timer;
        
        private IConnection _connection;
        private IModel _chanel;
        private IBasicProperties _basicProperties;

        public BlockScheduleService(ILogger<BlockScheduleService> logger, string rqmConnectionString,TimeSpan delay)
        {
            _logger = logger;
            _connectionFactory = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                DispatchConsumersAsync = true,
                Uri = new Uri(rqmConnectionString)
            };

            _timer = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer.Elapsed += DoWork;
        }

        private void DoWork(object sender, ElapsedEventArgs e)
        {
            var block = GetBlock();
            _logger.LogInformation($"send block {block.Number}");
            SendMessage(block);
        }

        private BlockInfo GetBlock()
        {
            return new BlockInfo
            {
                Number = new Random().Next(1,320000),
                Time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc)).TotalMilliseconds),
                Hash = $"0x{Guid.NewGuid().ToString().Replace("-","")}{Guid.NewGuid().ToString().Replace("-","")}"
            };
        }
        
        private void SendMessage(BlockInfo blockInfo)
        {
            var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(blockInfo));
            _chanel.ExchangeDeclare(EXCHANGE_NAME,"topic",true);
            _chanel.BasicPublish(EXCHANGE_NAME,ROUTING_KEY,_basicProperties,message);
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _connectionFactory.CreateConnection();
            _chanel = _connection.CreateModel();
            _basicProperties = _chanel.CreateBasicProperties();
            _basicProperties.Persistent = true;
            _basicProperties.DeliveryMode = 2;
            _timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Stop();
            return Task.CompletedTask;
        }
    }
}