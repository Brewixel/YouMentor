# Testing strategy

[Русская версия](testing.ru.md) · [Back to README](../README.md)

YouMentor uses domain, application, and integration tests to verify business behavior and persistence.

## Domain tests

Domain tests exercise the `Session` entity without ASP.NET Core or a database.

They cover:

- valid and invalid session creation;
- booking state transitions;
- cancellation state transitions;
- invalid identifiers and duration;
- restrictions based on the current time;
- UTC normalization.

## Application tests

Application tests verify handler behavior around current-user context, expected result types, domain rules, and persistence orchestration.

Fake implementations make authenticated and unauthenticated scenarios explicit.

## PostgreSQL integration tests

Integration tests use Testcontainers to start a real PostgreSQL container.

The test setup:

1. starts an isolated PostgreSQL instance;
2. builds the EF Core context with Npgsql and snake_case naming;
3. applies the actual application migrations;
4. executes the use case;
5. reads the persisted state through a separate context.

This catches provider-specific behavior that an in-memory database would not reproduce.

## Deterministic time

Production code depends on `TimeProvider` rather than calling the system clock directly.

Tests inject `FakeTimeProvider`, allowing scenarios around future and already-started sessions to use fixed timestamps without delays or flaky timing assumptions.

## Concurrency tests

The current suite verifies booking persistence and domain conflicts. Dedicated tests for Polly retry execution, retry exhaustion, and simultaneous competing bookings are the next testing iteration.

## Continuous integration

Pull requests targeting `dev` run GitHub Actions that restore dependencies, build the solution, and execute the test suite.

Run all tests locally with:

```bash
dotnet test YouMentor.slnx
```

Docker must be available because integration tests create a PostgreSQL container.
