# Архитектура

[English version](architecture.md) · [Вернуться к README](../README.ru.md)

YouMentor разделён на четыре проекта, зависимости которых направлены к доменному и application-слоям.

```text
Api
 └── Application
      ├── Domain
      └── application interfaces
           ▲
           │ реализует
           │
      Infrastructure
           └── PostgreSQL
```

## Проекты

### Domain

Содержит сущность `Session`, статусы сессии, переходы состояний, бизнес-инварианты и доменные типы результатов.

Доменная модель контролирует правила:

- сессия должна начинаться в будущем;
- забронировать можно только свободную будущую сессию;
- сессию нельзя отменить после начала;
- время нормализуется в UTC.

Проект не зависит от ASP.NET Core и persistence.

### Application

Содержит команды, запросы, MediatR handlers, FluentValidation validators, specifications, mapping и application-интерфейсы.

Endpoints передают сценарии handlers. Handler связывает контекст пользователя, доменные методы, persistence и resilience policies.

### Infrastructure

Содержит EF Core `DbContext`, конфигурацию PostgreSQL, mappings сущностей, миграции и реализации persistence-интерфейсов.

PostgreSQL остаётся источником истины. EF Core optimistic concurrency обнаруживает конкурирующие изменения.

### Api

Содержит Minimal API endpoints, dependency injection, JWT-аутентификацию, authorization policies, exception handling, Problem Details, OpenAPI, Scalar и регистрацию Polly pipeline.

## Поток команды

1. Аутентифицированный запрос поступает в Minimal API endpoint.
2. Endpoint отправляет команду через MediatR.
3. Валидация выполняется pipeline behavior.
4. Handler загружает актуальное состояние и вызывает доменное поведение.
5. EF Core сохраняет результат в PostgreSQL.
6. Ожидаемые ошибки преобразуются в типизированные результаты и HTTP-ответы.

## Границы архитектуры

Сейчас решение является единым развёртываемым backend-сервисом. Разделение на проекты используется для независимости бизнес-правил от ASP.NET Core и базы данных, а не для имитации распределённой системы.
