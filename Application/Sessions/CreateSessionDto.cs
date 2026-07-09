using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Sessions;

public record CreateSessionDto
{
	public DateTimeOffset StartTime { get; set; }
	public TimeSpan Duration { get; set; }
}
