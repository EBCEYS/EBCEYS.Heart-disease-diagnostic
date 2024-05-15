# DBAdapterService
# Описание:
Сервис для записи результатов диагностирования в базу данных PostgreSQL. Помимо этого, может запиывать клиентские данные, которые в последствиии можно использовать где-нибудь.
Сервис слушает выбранные каналы в RabbitMQ и если находит там сообщения для записи, то записывает их в базу данных.

# SQLscripts:
В данной директории хранятся sql скрипты выполняемые при запуске сервиса (если DoInitDataBaseOnStart - true).
Названия файлов скриптов не принципиальны. Сервис считывает только файлы *.sql.

# Описание конфигурационного файла:
```json
{
  "DataBaseConnection": { // Настройка связи с бд PostgreSQL
    "Host": "DB_PostgreSQL", // Имя хоста
    "Username": "adm", // Имя пользователя
    "Password": "adm", // пароль
    "Database": "DiagnoseResultsDB", // база данных
    "Port": "5432", // порт
    "DBRequestRetries": 1, // Кол-во повторов запросов на БД
    "DBRequestDelayBetweenRetries": 2000, // Задержка между повторами запросов
    "DoFullDBVacuumOnStart": true, // true - при запуске сервиса будет выполнен VACUUM FULL; иначе false
    "DoInitDataBaseOnStart": true // true - инициация базы данных при запуске сервиса; иначе false
  },
  "RabbitMQConsumers": [ // Описание Consumers RabbitMQ
    {
      "HostName": "rabbitmq", // имя хоста
      "QueueName": "db_queue", // имя очереди
      "UserName": "guest", // имя пользователя
      "Password": "guest", // пароль
      "Port": 5672 // порт
    }
  ]
}
```
### Доп инфо:
Сейчас в сервисе захардкожен "дебаг" режим, он нужен для дропа таблиц при запуске сервиса (не работает если DoInitDataBaseOnStart - false).