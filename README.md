# YouMentor — High-Load Mentorship Booking Platform 🚀

YouMentor is a backend solution designed to handle high-concurrency booking scenarios. The project serves as an architectural playground to demonstrate the evolution from a robust Modular Monolith to a distributed Microservices system.

The main focus of the project is solving complex engineering challenges: preventing race conditions, implementing flexible search patterns, and ensuring system observability.

## 🏗 Architecture & Patterns
The project is built on **Clean Architecture** principles, utilizing the **CQRS** pattern for strict separation of read and write operations.

* **Core:** Rich Domain Model (Domain-Driven Design).
* **Application:** CQRS via MediatR, Specification pattern for flexible queries, Result Pattern for error handling.
* **Infrastructure:** EF Core with PostgreSQL.
* **API:** ASP.NET Core Minimal APIs with endpoint grouping.

## 🛠 Tech Stack
* **Framework:** .NET 10 (ASP.NET Core Web API)
* **Database:** PostgreSQL / Entity Framework Core
* **Logic:** MediatR (CQRS), FluentValidation
* **Testing:** xUnit, Moq, FluentAssertions, Testcontainers
* **API Documentation:** OpenAPI with Scalar
* **CI/CD:** GitHub Actions (Semantic Release, PR Checks, Multi-env Deployments)
* **Future Stack:** RabbitMQ, Redis, OpenTelemetry, Dapper, Polly

## 🗺 Roadmap & Evolution
The project is being developed in stages, simulating real-world scaling requirements.

### Phase 1: Monolithic Foundation (Done ✅)
- [x] **Clean Architecture setup:** Domain, Application, Infrastructure, API layers.
- [x] **CQRS implementation:** Separating commands and queries via MediatR.
- [x] **Advanced filtering:** Specification pattern for dynamic DB queries.
- [x] **REST API:** Minimal APIs implementation via IEndpointRouteBuilder extensions.
- [x] **Concurrency control:** Optimistic Concurrency (RowVersion) to protect against overbooking.
- [x] **Custom Retry mechanism:** Handling DB concurrency conflicts gracefully.

### Phase 2: Optimization and Hardening (Current Focus 🚧)
- [x] **Containerization:** Docker and Docker Compose setup.
- [x] **Unit & Integration Testing:** Covering business logic with xUnit and Testcontainers.
- [x] **CI/CD:** Automated workflows via GitHub Actions.
- [ ] **AuthN & AuthZ:** Token-based authentication and policy-based authorization for protected endpoints.
- [ ] **Resilience:** Retry and Circuit Breaker policies using Polly.
- [ ] **Observability:** Structured logging (Serilog) and Tracing (OpenTelemetry).
- [ ] **Performance:** Introduce Dapper for "hot" read queries.

### Phase 3: Distributed System (Planned)
- [ ] **Microservices:** Extract Identity, Booking, and Notification services.
- [ ] **Asynchronous communication:** Event-driven architecture via RabbitMQ (MassTransit).
- [ ] **Caching:** Distributed caching based on Redis.

## 🚀 Getting Started

### Prerequisites
* .NET 10 SDK
* Docker Desktop (optional, for running with containers)

### Run locally via .NET CLI
1. Clone the repository:

```bash
git clone https://github.com/Brewixel/YouMentor
cd YouMentor
```

2. Update the connection string in Api/appsettings.json.

3. Apply migrations to the database:

```bash
dotnet ef database update --project Infrastructure --startup-project Api
```

4. Run the API:

```bash
dotnet run --project Api/Api.csproj
```

5. Navigate to http://localhost:5000/scalar/v1 to view the API documentation.

### Run via Docker Compose
1. Clone the repository:

```bash
git clone https://github.com/Brewixel/YouMentor
cd YouMentor
```

2. Simply run the following command in the root directory:

```bash
docker-compose up -d
```

2. The API will be available at http://localhost:5000 and the PostgreSQL database will be exposed on port 5435.
