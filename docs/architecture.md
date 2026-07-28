# Architecture

[Русская версия](architecture.ru.md) · [Back to README](../README.md)

YouMentor is organized into four projects with dependencies directed toward the domain and application layers.

```text
Api
 └── Application
      ├── Domain
      └── application interfaces
           ▲
           │ implemented by
           │
      Infrastructure
           └── PostgreSQL
```

## Projects

### Domain

Contains the `Session` entity, session statuses, state transitions, business invariants, and domain result types.

The domain model controls rules such as:

- a session must start in the future;
- only a free future session can be booked;
- a session cannot be cancelled after it starts;
- timestamps are normalized to UTC.

The domain project does not depend on ASP.NET Core or persistence.

### Application

Contains commands, queries, MediatR handlers, FluentValidation validators, specifications, mapping, and application interfaces.

Endpoints delegate use cases to handlers. Handlers coordinate authorization context, domain methods, persistence, and resilience policies.

### Infrastructure

Contains the EF Core `DbContext`, PostgreSQL configuration, entity mappings, migrations, and persistence implementations.

PostgreSQL is the source of truth. EF Core optimistic concurrency is used to detect competing updates.

### Api

Contains Minimal API endpoints, dependency injection, JWT authentication, authorization policies, exception handling, Problem Details mapping, OpenAPI, Scalar, and Polly pipeline registration.

## Request flow

A typical command follows this path:

1. An authenticated request reaches a Minimal API endpoint.
2. The endpoint sends a command through MediatR.
3. Validation runs through the pipeline behavior.
4. The handler loads current state and invokes domain behavior.
5. EF Core persists the result to PostgreSQL.
6. Expected failures are converted to typed results and mapped to HTTP responses.

## Architectural boundaries

The current solution is a single deployable backend. The separation into projects is used to keep business rules independent from ASP.NET Core and database details, not to simulate distributed services.
