using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace ConsumerWorker;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var conf = new ConsumerConfig
        {
            GroupId = _configuration["KafkaConfiguration:GroupId"],
            BootstrapServers = _configuration["KafkaConfiguration:BootstrapServers"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _logger.LogInformation("Start running at: {time}", DateTimeOffset.Now);
        var consumer = new ConsumerBuilder<Ignore, string>(conf).Build();

        try
        {

            consumer.Subscribe(_configuration["KafkaConfiguration:TopicToSubscribe"]);
            var cts = new CancellationTokenSource();
            _logger.LogInformation("Subscribe");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("TryConsume at: {time}", DateTimeOffset.Now);

                var message = consumer.Consume(cts.Token);
                _logger.LogInformation($"Mensagem: {message.Message.Value} recebida de {message.TopicPartitionOffset}");

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}

// public class Worker : BackgroundService
// {
//     private readonly ILogger<Worker> _logger;

//     public Worker(ILogger<Worker> logger)
//     {
//         _logger = logger;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
//             await Task.Delay(1000, stoppingToken);
//         }
//     }
// }
