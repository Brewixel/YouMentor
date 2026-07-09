using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Sessions;

public record SessionDto
{
	public Guid Id { get; set; }
	public Guid MentorId { get; set; }
	public DateTimeOffset StartTime { get; set; }
	public TimeSpan Duration { get; set; }
	public Guid? StudentId { get; set; }
	public SessionStatus Status { get; set; }
}
