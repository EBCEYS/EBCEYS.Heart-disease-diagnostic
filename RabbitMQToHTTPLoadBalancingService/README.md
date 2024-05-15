# RabbitMQToHTTPLoadBalancingService
## Описание
Сервис для балансировки нагрузки между RabbitMQ и HTTP сервером.

## Описание конфигурационного файла
```json
  "AllowedIPs": [ // адреса ai-flask-server-ов
    "FirstIp",
    "SecondIp"
  ],
  "RabbitMQConsumers": [ // конфигурация для Consumers RabbitMQ
    {
      "HostName": "HostName", // имя/адрес хоста
      "QueueName": "QueueName", // имя очереди
      "UserName": "UserName", // имя пользователя
      "Password": "Password", // пароль
      "Port": 5672, // порт
      "Timeout": 20 // таймаут для HTTP запросов
    }
  ]
```