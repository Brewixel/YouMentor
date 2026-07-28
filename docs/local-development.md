# Local development

[Русская версия](local-development.ru.md) · [Back to README](../README.md)

The current development flow runs PostgreSQL and Keycloak in Docker and starts the API through the .NET CLI.

## Requirements

- .NET 10 SDK
- Docker
- EF Core CLI tools

Install the EF Core CLI if needed:

```bash
dotnet tool install --global dotnet-ef
```

## 1. Clone the repository

```bash
git clone https://github.com/Brewixel/YouMentor.git
cd YouMentor
```

## 2. Start infrastructure

```bash
docker compose up -d postgres keycloak-db keycloak
```

The services are exposed locally as follows:

- PostgreSQL: `localhost:5435`
- Keycloak: `http://localhost:8080`

The development Keycloak administrator credentials configured in Docker Compose are:

- username: `admin`
- password: `admin`

These credentials are for local development only.

## 3. Create the application database

The application connection string expects a database named `sessions`.

Create it once:

```bash
docker compose exec postgres psql -U postgres -c "CREATE DATABASE sessions;"
```

If the database already exists, skip this step.

## 4. Configure Keycloak

Open `http://localhost:8080` and create:

1. Realm: `youmentor`
2. API client or audience: `youmentor-api`
3. Realm roles: `mentor` and `student`
4. Development users with the required roles
5. A token mapper that exposes assigned roles in a top-level `roles` claim

The API development configuration expects:

```text
Authority: http://localhost:8080/realms/youmentor
Audience: youmentor-api
Role claim: roles
```

Realm import is not automated yet.

## 5. Apply migrations

```bash
dotnet ef database update \
  --project Infrastructure \
  --startup-project Api
```

## 6. Run the API

```bash
dotnet run --project Api/Api.csproj
```

The API starts at:

```text
http://localhost:5000
```

Scalar API documentation is available at:

```text
http://localhost:5000/scalar/v1
```

Session endpoints require a Bearer access token issued by the configured Keycloak realm.

## 7. Run tests

```bash
dotnet test YouMentor.slnx
```

Docker must be running because integration tests start an isolated PostgreSQL container through Testcontainers.

## Stop infrastructure

```bash
docker compose down
```

To also remove local database volumes:

```bash
docker compose down -v
```
