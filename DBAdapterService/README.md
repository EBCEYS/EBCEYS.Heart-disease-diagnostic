# DBAdapterService
# ��������:
������ ��� ������ ����������� ���������������� � ���� ������ PostgreSQL. ������ �����, ����� ��������� ���������� ������, ������� � ������������ ����� ������������ ���-������.
������ ������� ��������� ������ � RabbitMQ � ���� ������� ��� ��������� ��� ������, �� ���������� �� � ���� ������.

# SQLscripts:
� ������ ���������� �������� sql ������� ����������� ��� ������� ������� (���� DoInitDataBaseOnStart - true).
�������� ������ �������� �� �������������. ������ ��������� ������ ����� *.sql.

# �������� ����������������� �����:
```json
{
  "DataBaseConnection": { // ��������� ����� � �� PostgreSQL
    "Host": "DB_PostgreSQL", // ��� �����
    "Username": "adm", // ��� ������������
    "Password": "adm", // ������
    "Database": "DiagnoseResultsDB", // ���� ������
    "Port": "5432", // ����
    "DBRequestRetries": 1, // ���-�� �������� �������� �� ��
    "DBRequestDelayBetweenRetries": 2000, // �������� ����� ��������� ��������
    "DoFullDBVacuumOnStart": true, // true - ��� ������� ������� ����� �������� VACUUM FULL; ����� false
    "DoInitDataBaseOnStart": true // true - ��������� ���� ������ ��� ������� �������; ����� false
  },
  "RabbitMQConsumers": [ // �������� Consumers RabbitMQ
    {
      "HostName": "rabbitmq", // ��� �����
      "QueueName": "db_queue", // ��� �������
      "UserName": "guest", // ��� ������������
      "Password": "guest", // ������
      "Port": 5672 // ����
    }
  ]
}
```
### ��� ����:
������ � ������� ����������� "�����" �����, �� ����� ��� ����� ������ ��� ������� ������� (�� �������� ���� DoInitDataBaseOnStart - false).