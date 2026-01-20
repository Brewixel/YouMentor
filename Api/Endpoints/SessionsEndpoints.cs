using Api.Extensions;
using Application.Sessions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Endpoints
{
	public static class SessionsEndpoints
	{
		public static void MapSessionsEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app.MapGroup("api/sessions");
			group.MapPost("/", CreateSession);
			group.MapGet("/", GetSessions);
		}

		private static async Task<Results<Ok<Guid>, NotFound, BadRequest<string>>> CreateSession(
			ISender mediator, 
			CancellationToken ct, 
			[FromBody] CreateSessionDto session)
		{
			var result = await mediator.Send(new Create.Command { Session = session });
			return result.ToHttpResult();
		}

		private static async Task<Results<Ok<List<SessionDto>>, NotFound, BadRequest<string>>> GetSessions(
			ISender mediator,
			CancellationToken ct)
		{
			var result = await mediator.Send(new List.Query());
			return result.ToHttpResult();
		}
	}
}
