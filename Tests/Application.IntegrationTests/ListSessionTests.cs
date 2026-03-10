using Application.Sessions;
using Domain.Entities;
using Domain.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

public class ListSessionTests : IntegrationTestBase
{
	[Fact]
	public async Task Handle_Should_ReturnAllSessions_When_NoFilters()
	{
		// Arrange
		var sessions = await SeedSessionsAsync();
		var query = new List.Query()
		{
			MentorId = null,
			OnlyFree = false
		};

		await using var setupContext = BuildContext();
		var mapper = new SessionMapper();
		var handler = new List.Handler(setupContext, mapper);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		AssertSuccessAndEquivalent(result, sessions);
	}

	[Fact]
	public async Task Handle_Should_ReturnMentorSessions_When_MentorFilter()
	{
		// Arrange
		var sessions = await SeedSessionsAsync();
		var mentorId = sessions[0].MentorId;
		var mentorSessions = sessions.Where(x => x.MentorId == mentorId);
		var query = new List.Query()
		{
			MentorId = mentorId,
			OnlyFree = false
		};

		await using var setupContext = BuildContext();
		var mapper = new SessionMapper();
		var handler = new List.Handler(setupContext, mapper);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		AssertSuccessAndEquivalent(result, mentorSessions);
	}

	[Fact]
	public async Task Handle_Should_ReturnAllFreeSessions_When_OnlyFreeFilter()
	{
		// Arrange
		var sessions = await SeedSessionsAsync();
		var freeSessions = sessions.Where(x => x.Status == SessionStatus.Free);
		var query = new List.Query()
		{
			MentorId = null,
			OnlyFree = true
		};

		await using var setupContext = BuildContext();
		var mapper = new SessionMapper();
		var handler = new List.Handler(setupContext, mapper);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		AssertSuccessAndEquivalent(result, freeSessions);
	}

	[Fact]
	public async Task Handle_Should_ReturnFreeMentorSessions_When_OnlyFreeAndMentorFilter()
	{
		// Arrange
		var sessions = await SeedSessionsAsync();
		var mentorId = sessions[0].MentorId;
		var freeMentorSessions = sessions.Where(x =>
			x.Status == SessionStatus.Free && x.MentorId == mentorId);

		var query = new List.Query()
		{
			MentorId = mentorId,
			OnlyFree = true
		};

		await using var setupContext = BuildContext();
		var mapper = new SessionMapper();
		var handler = new List.Handler(setupContext, mapper);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		AssertSuccessAndEquivalent(result, freeMentorSessions);
	}

	private async Task<List<SessionDto>> SeedSessionsAsync()
	{
		Guid mentorId1 = Guid.NewGuid();
		Guid mentorId2 = Guid.NewGuid();
		var startDate = DateTime.UtcNow.AddDays(1);
		var duration = TimeSpan.FromDays(1);

		var sessions = new List<SessionDto>
		{
			// for first mentor
			new SessionDto
			{
				Id = Guid.NewGuid(),
				MentorId = mentorId1,
				StudentId = null,
				StartTime = startDate,
				Duration = duration,
				Status = SessionStatus.Free
			},
			new SessionDto
			{
				Id = Guid.NewGuid(),
				MentorId = mentorId1,
				StudentId = null,
				StartTime = startDate,
				Duration = duration,
				Status = SessionStatus.Free
			},
			new SessionDto
			{
				Id = Guid.NewGuid(),
				MentorId = mentorId1,
				StudentId = Guid.NewGuid(),
				StartTime = startDate,
				Duration = duration,
				Status = SessionStatus.Booked
			},

			// for second mentor
			new SessionDto
			{
				Id = Guid.NewGuid(),
				MentorId = mentorId2,
				StudentId = null,
				StartTime = startDate,
				Duration = duration,
				Status = SessionStatus.Free
			}
		};

		await InsertSessionsBulkAsync(sessions);
		return sessions;
	}

	private async Task InsertSessionsBulkAsync(List<SessionDto> sessions)
	{
		if (!sessions.Any())
			return;

		await using var context = BuildContext();

		var parameters = new List<object>();
		var valueTuples = new List<string>();
		int paramIndex = 0;

		foreach (var session in sessions)
		{
			// Generate placeholders for EF Core in the format: {0}, {1}, {2}...
			valueTuples.Add($"({{{paramIndex}}}, {{{paramIndex + 1}}}, {{{paramIndex + 2}}}, {{{paramIndex + 3}}}, {{{paramIndex + 4}}}, {{{paramIndex + 5}}})");

			parameters.Add(session.Id);
			parameters.Add(session.MentorId);
			parameters.Add(session.StudentId!);
			parameters.Add(session.StartTime);
			parameters.Add(session.Duration);
			parameters.Add((int)session.Status);

			paramIndex += 6;
		}

		var sql = $@"
        INSERT INTO ""Sessions"" (""Id"", ""MentorId"", ""StudentId"", ""StartTime"", ""Duration"", ""Status"") 
        VALUES {string.Join(", ", valueTuples)}";

		await context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
	}

	private void AssertSuccessAndEquivalent(Result<List<SessionDto>> result, IEnumerable<SessionDto> expected)
	{
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);
		result.Value.Should().NotBeNull();
		result.Value.Should().BeEquivalentTo(expected, options => options
			.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1)))
			.WhenTypeIs<DateTime>()
		);
	}
}
