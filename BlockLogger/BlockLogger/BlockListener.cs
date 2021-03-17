using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace BlockLogger
{
    public class BlockListener:IHostedService
    {
        private const string EXCHANGE_NAME = "e.b.forward";
        private const string ROUTING_KEY = "r.b.app.block";
        private const string QUEUE_NAME = "q.b.block-logger.block-info";
        
        private readonly ILogger<BlockListener> _logger;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IDbConnection _dbConnection;
        private readonly ConnectionMultiplexer _redisClient;

        private IConnection _connection;
        private IModel _chanel;
        private EventingBasicConsumer _consumer;

        public BlockListener(ILogger<BlockListener> logger, string rmqConnection, string pgConnection,
            string redisConnection)
        {
            _logger = logger;
            _connectionFactory = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                Uri = new Uri(rmqConnection)
            };

            _dbConnection = new NpgsqlConnection(pgConnection);
            _redisClient = ConnectionMultiplexer.Connect(redisConnection);
        }

        private long Increment()
        {
            var db = _redisClient.GetDatabase();

            return db.StringIncrement("block.received.count", 1);
        }

        private void SaveBlock(BlockInfo blockInfo)
        {
            var sqlQuery = @"INSERT INTO blocks (number,time,hash)
VALUES (@Number,@Time,@Hash)
ON CONFLICT(number) DO
UPDATE SET 
time = EXCLUDED.time,
hash = EXCLUDED.hash;";

            _dbConnection.Execute(sqlQuery, blockInfo);
        }

        private void OnMessage(object sender, BasicDeliverEventArgs eventArgs)
        {
            var blockInfo = JsonConvert.DeserializeObject<BlockInfo>(Encoding.UTF8.GetString(eventArgs.Body.ToArray()));
            _logger.LogInformation($"Receive block {blockInfo.Number}");
            
            _logger.LogInformation($"Try save block {blockInfo.Number}");
            SaveBlock(blockInfo);

            var result = Increment();
            _logger.LogInformation($"Increment result {result} for block {blockInfo.Number}");
        }

        private void Subscribe()
        {
            _chanel.ExchangeDeclare(EXCHANGE_NAME,"topic",true);
            
            _chanel.QueueDeclare(QUEUE_NAME, true);
            _chanel.QueueBind(QUEUE_NAME,EXCHANGE_NAME,ROUTING_KEY);
            _logger.LogInformation($"Subscribe {QUEUE_NAME} to {EXCHANGE_NAME} by {ROUTING_KEY}");
            _chanel.BasicConsume(consumer: _consumer, queue: QUEUE_NAME, autoAck: true);
        }

        private void Unsubscribe()
        {
            _consumer.OnCancel();
            _logger.LogInformation($"Unsubscribe from {QUEUE_NAME}");
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _connectionFactory.CreateConnection();
            _chanel = _connection.CreateModel();
            _consumer = new EventingBasicConsumer(_chanel);
            _consumer.Received += OnMessage;
            _dbConnection.Open();
            Subscribe();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _dbConnection.Close();
            Unsubscribe();
            return Task.CompletedTask;
        }
    }
}