using Api.Extensions;
using Application.Sessions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class SessionsEndpoints
{
	public static void MapSessionsEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("api/sessions");
		group.MapPost("/", CreateSession);
		group.MapGet("/", GetAllSessions);
		group.MapGet("/free", GetFreeSessions);
		group.MapGet("/by-mentor/{mentorId:guid}", GetMentorSessions);
	}

	private static async Task<Results<Ok<Guid>, NotFound, BadRequest<string>>> CreateSession(
		ISender mediator,
		CancellationToken ct,
		[FromBody] CreateSessionDto session)
	{
		var result = await mediator.Send(
			new Create.Command { Session = session });
		return result.ToHttpResult();
	}

	private static async Task<Results<Ok<List<SessionDto>>, NotFound, BadRequest<string>>> GetAllSessions(
		ISender mediator,
		CancellationToken ct)
	{
		var result = await mediator.Send(new List.Query(), ct);
		return result.ToHttpResult();
	}

	private static async Task<Results<Ok<List<SessionDto>>, NotFound, BadRequest<string>>> GetFreeSessions(
		ISender mediator,
		CancellationToken ct)
	{
		var result = await mediator.Send(
			new List.Query { OnlyFree = true },
			ct);
		return result.ToHttpResult();
	}

	private static async Task<Results<Ok<List<SessionDto>>, NotFound, BadRequest<string>>> GetMentorSessions(
		ISender mediator,
		CancellationToken ct,
		Guid mentorId)
	{
		var result = await mediator.Send(
			new List.Query { MentorId = mentorId },
			ct);
		return result.ToHttpResult();
	}
}
