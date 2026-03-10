using Application.Sessions;
using Domain.Entities;
using Domain.Results;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace Application.Tests;

public class BookSessionTests : IntegrationTestBase
{
	[Fact]
	public async Task Handle_Should_BookSession_When_EverythingIsOk()
	{
		// Arrange
		var sessionId = await CreateSessionAsync();
		var command = new Book.Command()
		{
			SessionId = sessionId,
			StudentId = Guid.NewGuid(),
		};
		await using var setupContext = BuildContext();
		var handler = new Book.Handler(setupContext, NullLogger<Book.Handler>.Instance);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);

		var session = await GetSessionAsync(sessionId);
		session!.Status.Should().Be(SessionStatus.Booked);
	}

	[Fact]
	public async Task Handle_Should_ReturnNotFound_When_SessionDoesNotExist()
	{
		// Arrange
		var command = new Book.Command()
		{
			SessionId = Guid.NewGuid(),
			StudentId = Guid.NewGuid(),
		};
		await using var setupContext = BuildContext();
		var handler = new Book.Handler(setupContext, NullLogger<Book.Handler>.Instance);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.NotFound);
	}

	[Fact]
	public async Task Handle_Should_ReturnValidation_When_StudentIsEmpty()
	{
		// Arrange
		var sessionId = await CreateSessionAsync();
		var studentId = Guid.Empty;

		await using var setup = BuildBookSetup(sessionId, studentId: studentId);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Validation);

		var session = await GetSessionAsync(sessionId);
		session!.Status.Should().Be(SessionStatus.Free);
		session.StudentId.Should().BeNull();
	}

	[Fact]
	public async Task Handle_Should_ReturnConflict_When_SessionIsAlreadyBooked()
	{
		// Arrange
		var sessionId = await CreateSessionAsync();
		await using var setup1 = BuildBookSetup(sessionId);
		await using var setup2 = BuildBookSetup(sessionId);

		// Act
		var result1 = await setup1.Handler.Handle(setup1.Command, CancellationToken.None);
		var result2 = await setup2.Handler.Handle(setup2.Command, CancellationToken.None);

		// Assert
		result1.IsSuccess.Should().BeTrue(result1.ErrorInfo?.Message);

		result2.IsSuccess.Should().BeFalse();
		result2.ErrorInfo?.Type.Should().Be(ErrorType.Conflict);

		var session = await GetSessionAsync(sessionId);
		session!.StudentId.Should().Be(setup1.Command.StudentId);
	}

	[Fact]
	public async Task Handle_Should_RetryAndSave_When_RetriesCountLessOrEqualThanMax()
	{
		// Arrange
		var initTasks = Enumerable.Range(1, Book.Handler.MaxRetries)
			.Select(
				async num => await BuildConcurentDataAsync(num)
			);

		var concurentDict = (await Task.WhenAll(initTasks))
			.ToList();

		// Act
		foreach (var concurentData in concurentDict)
		{
			var setup = concurentData.Setup;
			concurentData.Result = await setup.Handler.Handle(setup.Command, CancellationToken.None);
		}

		// Assert
		foreach(var concurentData in concurentDict)
		{
			var result = concurentData.Result;
			var sessionId = concurentData.SessionId;
			var logger = concurentData.Setup.Logger!;
			var retriesCount = concurentData.RetriesCount;

			result.IsSuccess.Should().BeTrue($"{result.ErrorInfo?.Message}. Retries count: {retriesCount}");
			var session = await GetSessionAsync(sessionId);
			session!.Status.Should().Be(SessionStatus.Booked);

			logger.Messages.Where(x => x.Contains("Concurrency conflict"))
				.Should().HaveCount(retriesCount);
		}
	}

	[Fact]
	public async Task Handle_Should_ReturnFailure_When_RetriesCountMoreThanMax()
	{
		// Arrange
		var retriesCount = Book.Handler.MaxRetries + 1;
		var concurentData = await BuildConcurentDataAsync(retriesCount);

		// Act
		var setup = concurentData.Setup;
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		var sessionId = concurentData.SessionId;

		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo!.Type.Should().Be(ErrorType.Failure);

		var session = await GetSessionAsync(sessionId);
		session!.Status.Should().Be(SessionStatus.Free);
		session.StudentId.Should().BeNull();
	}

	private BookSetup BuildBookSetup(Guid sessionId,
		Guid? studentId = null,
		FakeLogger<Book.Handler>? logger = null,
		IInterceptor[]? contextInterceptors = null)
	{
		studentId ??= Guid.NewGuid();
		contextInterceptors ??= [];
		ILogger<Book.Handler> abstractLogger = logger != null
			? logger
			: NullLogger<Book.Handler>.Instance;

		var command = new Book.Command()
		{
			SessionId = sessionId,
			StudentId = studentId.Value,
		};

		var setupContext = BuildContext(contextInterceptors);
		var handler = new Book.Handler(setupContext, abstractLogger);

		return new BookSetup
		{
			Command =  command,
			Handler = handler,
			Context = setupContext,
			Logger = logger
		};
	}

	private async Task<ConcurentData> BuildConcurentDataAsync(int retriesCount)
	{
		var sessionId = await CreateSessionAsync();
		var studentId = Guid.NewGuid();

		return new ConcurentData
		{
			SessionId = sessionId,
			StudentId = studentId,
			Setup = BuildBookSetup(sessionId,
				logger: new FakeLogger<Book.Handler>(),
				contextInterceptors: [new ConcurrencyExceptionInterceptor(retriesCount)]
			),
			RetriesCount = retriesCount
		};
	}

	private async Task<Guid> CreateSessionAsync()
	{
		var sessionDto = new CreateSessionDto()
		{
			MentorId = Guid.NewGuid(),
			StartTime = DateTime.UtcNow.AddDays(1),
			Duration = TimeSpan.FromHours(1),
		};
		var command = new Create.Command() { Session = sessionDto };

		await using var context = BuildContext();
		var handler = new Create.Handler(context);

		return (await handler.Handle(command, CancellationToken.None)).Value;
	}

	public class BookSetup : IAsyncDisposable
	{
		public required Book.Command Command { get; set; }
		public required Book.Handler Handler { get; set; }
		public required AppDbContext Context { get; set; }
		public FakeLogger<Book.Handler>? Logger { get; set; }

		public async ValueTask DisposeAsync()
		{
			await Context.DisposeAsync();
		}
	}

	record ConcurentData
	{
		public required BookSetup Setup { get; set; }
		public required Guid SessionId { get; set; }
		public required Guid StudentId { get; set; }
		public Result Result { get; set; } = null!;
		public required int RetriesCount { get; set; }
	}
}
