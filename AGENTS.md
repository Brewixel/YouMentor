# Repository Guidelines

## Project Structure & Module Organization

YouMentor is a .NET 10 Clean Architecture backend. The solution is `YouMentor.slnx`.

- `Api/`: Minimal API entry point, endpoints, middleware, auth, settings, and Dockerfile.
- `Application/`: CQRS handlers, DTOs, validation, interfaces, specifications, and mapping.
- `Domain/`: core entities and result types; keep business invariants here.
- `Infrastructure/`: EF Core persistence, PostgreSQL configuration, migrations, and design-time context factory.
- `Tests/`: xUnit test projects split into `Domain.Tests`, `Application.UnitTests`, and `Application.IntegrationTests`.

Do not edit generated `bin/` or `obj/` output. EF migrations live in `Infrastructure/Migrations`.

## Codex Preferences

- When the user asks to remember or save a preference in memory, write it in this `AGENTS.md` file.
- Create separate files for classes and other types.
- Do not run build or test commands unless the user explicitly asks for them.

## Build, Test, and Development Commands

- `dotnet restore YouMentor.slnx`: restore NuGet packages.
- `dotnet build YouMentor.slnx --no-restore`: build all projects.
- `dotnet test YouMentor.slnx --no-build`: run tests after a build.
- `dotnet test Tests/Application.UnitTests/Application.UnitTests.csproj`: run one test suite while iterating.
- `dotnet ef database update --project Infrastructure --startup-project Api`: apply EF Core migrations.
- `dotnet run --project Api/Api.csproj`: run the API locally; Scalar docs are exposed at `/scalar/v1`.
- `docker-compose up -d`: start the API and PostgreSQL using the repository compose files.

Integration tests use Testcontainers and require Docker.

## Coding Style & Naming Conventions

Follow `.editorconfig`: tabs with width 4, CRLF line endings, UTF-8, trim trailing whitespace, and insert a final newline. C# uses nullable reference types and implicit usings. Prefer file-scoped namespaces.

Use PascalCase for public types and methods, camelCase for locals and parameters, and `Async` suffixes when applicable. Keep CQRS files organized by feature under `Application/<Feature>/`, for example `Application/Sessions/Book.cs` and validators under `Validators/`.

## Testing Guidelines

Tests use xUnit, FluentAssertions, Moq for unit tests, and Testcontainers PostgreSQL for integration tests. Name test classes after the behavior under test, such as `SessionTests` or `BookCommandValidatorTests`. Add integration tests when EF queries, transactions, or concurrency behavior are involved.

Run `dotnet test YouMentor.slnx` before opening a PR.

## Commit & Pull Request Guidelines

Use Conventional Commits, matching existing history: `feat: Add session cancellation`, `fix: Make runtime query generation match design-time`, `test: Cover ValidationBehavior`. PR titles targeting `dev` must use one of `feat`, `fix`, `test`, `refactor`, `ci`, `docs`, `chore`, or `wip`, and the subject must start with a capital letter.

For PRs, include a short description, testing notes, linked issues when relevant, and screenshots only for visible API/docs changes. CI restores, builds, and tests `YouMentor.slnx`.

## Security & Configuration Tips

Keep secrets out of `appsettings.json`; use local development settings, environment variables, or user secrets. Check connection strings before running migrations, especially against shared databases.
