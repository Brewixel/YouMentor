# YouMentor — Mentorship Session Booking Backend

[Русская версия](README.ru.md)

YouMentor is an ASP.NET Core backend for creating, booking, and cancelling mentorship sessions.

The project focuses on business rules, authorization, concurrent data changes, and integration testing with PostgreSQL.

## Highlights

- ASP.NET Core Minimal APIs on .NET 10
- PostgreSQL and Entity Framework Core
- JWT authentication with Keycloak
- Role- and policy-based authorization
- Domain-level business rules
- Optimistic concurrency for competing bookings
- Targeted retry with Polly
- Unit and integration tests with Testcontainers
- Docker-based local infrastructure
- GitHub Actions CI

## Core use cases

| Actor | Operation | Main rule |
|---|---|---|
| Mentor | Create a session | The session must start in the future |
| Student | Book a session | Only a free future session can be booked |
| Mentor | Cancel a session | Only the session owner can cancel it before it starts |
| Multiple students | Book the same session | Only one booking can be persisted |

The `Session` domain model controls state transitions and prevents invalid operations such as booking an occupied slot or cancelling a session after its start time.

## Concurrency-safe booking

Two students may attempt to book the same free session simultaneously.

YouMentor uses EF Core optimistic concurrency and a targeted Polly retry pipeline. After a concurrency conflict, the application clears the stale tracked state, reloads the session, and evaluates the business rules again.

The pipeline retries only `DbUpdateConcurrencyException`. Validation, authorization, and ordinary domain conflicts are not retried.

[Read more about concurrency handling](docs/concurrency.md)

## Authorization

The API validates JWT access tokens issued by Keycloak.

- Mentors can create sessions.
- Students can book sessions.
- Only the mentor who owns a session can cancel it.
- All session endpoints require an authenticated user.

[Read more about authentication and authorization](docs/authentication.md)

## Architecture

```text
API
 └── Application
      ├── Domain
      └── Application interfaces
           ▲
           │ implemented by
           │
      Infrastructure
           └── PostgreSQL
```

The solution contains four main projects:

- `Domain` — entities, state transitions, invariants, and result types
- `Application` — commands, queries, validation, specifications, and use-case orchestration
- `Infrastructure` — EF Core, PostgreSQL configuration, and migrations
- `Api` — endpoints, authentication, authorization, error handling, and dependency injection

[Read more about the architecture](docs/architecture.md)

## Tech stack

`.NET 10` · `ASP.NET Core` · `PostgreSQL` · `Entity Framework Core` · `Keycloak` · `MediatR` · `FluentValidation` · `Polly` · `xUnit` · `Testcontainers` · `Docker`

## Local development

See the [local development guide](docs/local-development.md) for infrastructure setup, Keycloak configuration, database migrations, API startup, and test commands.

## Testing

Integration tests run against a real PostgreSQL container and apply the actual EF Core migrations.

Time-dependent scenarios use `TimeProvider` and `FakeTimeProvider`, keeping the tests deterministic.

Pull requests targeting the `dev` branch are built and tested through GitHub Actions.

[Read more about the testing strategy](docs/testing.md)

## Current scope

The current implementation covers:

- session creation, search, booking, and cancellation;
- JWT authentication and role-based authorization;
- resource ownership checks;
- optimistic concurrency;
- targeted retry for booking conflicts;
- domain, application, and integration tests.

The next iterations focus on completing concurrency tests, adding bounded retry to cancellation where appropriate, and publishing `SessionBooked` through MassTransit and RabbitMQ.
