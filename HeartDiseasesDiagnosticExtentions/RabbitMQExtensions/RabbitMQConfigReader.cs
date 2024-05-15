using EBCEYS.RabbitMQ.Configuration;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace HeartDiseasesDiagnosticExtentions.RabbitMQExtensions
{
    /*"RPCRabbitMQConfig": { //настройки rabbitmq клиента для rpc_queue
    "QueueConfiguration": {
      "Queue": "rpc_queue"
    },
    "ConnectionFactory": {
      "HostName": "rabbitmq",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest"
    }
  },*/
    public static class RabbitMQConfigReader
    {
        public static RabbitMQConfiguration GetRabbitMQConfiguration(this IConfiguration config, string paramName)
        {
            RabbitMQConfigurationBuilder builder = new();
            builder.AddConnectionFactory(config.GetSection($"{paramName}:ConnectionFactory").Get<ConnectionFactory>()!);

            QueueConfiguration queueConfiguration = new(config.GetValue<string>($"{paramName}:QueueConfiguration:QueueName")
                , config.GetValue<bool?>($"{paramName}:QueueConfiguration:Durable") ?? false
                , config.GetValue<bool?>($"{paramName}:QueueConfiguration:Exclusive") ?? false
                , config.GetValue<bool?>($"{paramName}:QueueConfiguration:AutoDelete") ?? true);

            builder.AddQueueConfiguration(queueConfiguration);
            ExchangeConfiguration? exconf = config.GetSection($"{paramName}:ExchangeConfiguration").Get<ExchangeConfiguration>();
            if ( exconf != null )
            {
                builder.AddExchangeConfiguration(exconf);
            }
            return builder.Build();
        }
    }
}
