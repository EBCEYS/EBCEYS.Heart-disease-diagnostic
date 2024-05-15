# AuthService
WEB API для работы с пользователями в системе.

## Описание конфигурационного файла:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "JwtAuth": { //настройки токена
    "Issuer": "AuthService",
    "Audience": "AuthService"
  },
  "RabbitMQSettings": { // настройки rabbitMQ клиента
    "Queue": "users_db_queue",
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "Timeout": 20
  },
  "TokenExpireTime": 360, // время жизни токена в минутах
  "AllowedHosts": "*",
  "Port": 5000 //Порт, который слушает сервис (возможно в будущем я его захардкожу).
}
```

## Описание работы системы
### RabbitMQ
При получении запроса сервер стучится в [UsersDBAdapter](https://github.com/EBCEYS/Heart-Diseases-Diagnostic-WEB-API/tree/main/UsersDBAdapter).
Пример взаимодействия представлен на следующей диаграмме:
![authorize](https://github.com/EBCEYS/Heart-Diseases-Diagnostic-WEB-API/blob/main/LoginRequest.png)
#### ВАЖНОЕ!
В данном случае паблишер RabbitMQ посылает синхронные запросы(!!!), по этому важно указывать таймаут ожидания ответа!