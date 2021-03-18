# docker-compose-example

1) PostgresSQL (порт внутри докера 5432)
   Конфигурация распологаеться по пути configs/postgresql/.env.pg_file
   POSTGRES_USER -- пользователь бд 
   POSTGRES_PASSWORD -- пароль для юзера
   POSTGRES_DB -- имя бд
   Скрипт инициализации БД (создания таблиц) находить по пути configs/postgresql/init.sql
   Все волумы сохраняються по пути volumes/postgresql/
   
2) RabitMQ (порт внутри докера 5672 и 15672)
    RABBITMQ_DEFAULT_USER -- default user 
    RABBITMQ_DEFAULT_PASS -- default password	
    Все волумы сохраняються по пути volumes/rabbitmq/

3) Redis (порт внутри докера 6379)
    Ипользует дефолтные конфиги.
    Все волумы сохраняються по пути volumes/redis/
    
4) BlockProvider (порт внутри докера 80)
    Конфигурируеться через переменные окружения
    RMQ_CONNECTION -- строка подключения к rabbitMQ 
    BLOCK_CALL_INTERVAL -- частота опроса ноды и отправки блока в RabbitMQ, задаеться в секундах
    
5)BlockLogger (порт внутри докера 80)
    Конфигурируеться через переменные окружения
    RMQ_CONNECTION -- строка подключения к RbbitMQ 
    PG_CONNECTION -- строка подключения к PtgresSQL
    REDIS_CONNECTION -- строка подключения к Redis

Приложения 4 и 5 по умолчанию слушают порт 80 по http
ASPNETCORE_URLS --  пример https://+:5001  или http://+:80
