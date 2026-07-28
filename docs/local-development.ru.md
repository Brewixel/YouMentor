# Локальная разработка

[English version](local-development.md) · [Вернуться к README](../README.ru.md)

Текущий сценарий разработки запускает PostgreSQL и Keycloak в Docker, а API — через .NET CLI.

## Требования

- .NET 10 SDK
- Docker
- EF Core CLI tools

При необходимости установить EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

## 1. Клонировать репозиторий

```bash
git clone https://github.com/Brewixel/YouMentor.git
cd YouMentor
```

## 2. Запустить инфраструктуру

```bash
docker compose up -d postgres keycloak-db keycloak
```

Сервисы доступны локально:

- PostgreSQL: `localhost:5435`
- Keycloak: `http://localhost:8080`

Учётные данные администратора Keycloak из Docker Compose:

- username: `admin`
- password: `admin`

Они предназначены только для локальной разработки.

## 3. Создать базу приложения

Connection string приложения ожидает базу `sessions`.

Однократно создать её:

```bash
docker compose exec postgres psql -U postgres -c "CREATE DATABASE sessions;"
```

Если база уже существует, этот шаг можно пропустить.

## 4. Настроить Keycloak

Открыть `http://localhost:8080` и создать:

1. Realm: `youmentor`
2. API client или audience: `youmentor-api`
3. Realm roles: `mentor` и `student`
4. Тестовых пользователей с нужными ролями
5. Token mapper, который помещает назначенные роли в верхнеуровневый claim `roles`

Development-конфигурация API ожидает:

```text
Authority: http://localhost:8080/realms/youmentor
Audience: youmentor-api
Role claim: roles
```

Автоматический импорт realm пока не настроен.

## 5. Применить миграции

```bash
dotnet ef database update \
  --project Infrastructure \
  --startup-project Api
```

## 6. Запустить API

```bash
dotnet run --project Api/Api.csproj
```

API будет доступен по адресу:

```text
http://localhost:5000
```

Документация Scalar:

```text
http://localhost:5000/scalar/v1
```

Endpoints сессий требуют Bearer access token, выпущенный настроенным Keycloak realm.

## 7. Запустить тесты

```bash
dotnet test YouMentor.slnx
```

Docker должен быть запущен, потому что integration-тесты создают изолированный PostgreSQL-контейнер через Testcontainers.

## Остановить инфраструктуру

```bash
docker compose down
```

Удалить также локальные volumes базы данных:

```bash
docker compose down -v
```
