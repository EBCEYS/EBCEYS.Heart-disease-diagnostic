# UsersDBAdapter
Адаптер сервис для базы данных для работы с пользователями.

## Описание конфигурационного файла:
```json
{
  "UseConnectionString": false, //true - будет использоваться DefaultConnection из ConnectionStrings; иначе DataBaseConnection
  "ConnectionStrings": {
    "DefaultConnection": "Host=DB_PostgreSQL;Username=adm;Database=DiagnoseResultsDB;Port=5432"
  },
  "DataBaseConnection": { // описание соединения с PostgreSQL
    "Host": "DB_PostgreSQL", // имя хоста
    "Username": "adm", // имя пользователя
    "Password": "adm", // пароль
    "Database": "DiagnoseResultsDB", // база данных
    "Port": "5432", // порт
    "DBRequestRetries": 1, // кол-во повторов запросов к БД
    "DBRequestDelayBetweenRetries": 2000, // задержка между запросами к БД
    "DoInitDataBaseOnStart": true // true - инициировать бд при старте сервиса; иначе false
  },
  "RabbitMQConsumers": [ // конфигурация Consumers RabbitMQ
    {
      "HostName": "rabbitmq", // имя хоста
      "Queue": "users_db_queue", // имя очереди
      "UserName": "guest", // имя пользователя
      "Password": "guest", // пароль
      "Port": 5672 // порт
    }
  ],
  "GetRolesFromDataBase": false, //true - получать роли из БД; false - записывать роли из списка Roles в базу данных
  "Roles": [
    "adm",
    "usr"
  ]
}
```

## Описание работы системы
### RabbitMQ
При получении запроса от [AuthService](https://github.com/EBCEYS/Heart-Diseases-Diagnostic-WEB-API/tree/main/AuthService) выполняет определенный метод.
Запросы синхронные, так что должен на них отвечать.