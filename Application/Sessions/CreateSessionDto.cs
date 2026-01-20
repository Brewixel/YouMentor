using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Sessions;

public class CreateSessionDto
{
	public Guid MentorId { get; set; }
	public DateTime StartTime { get; set; }
	public TimeSpan Duration { get; set; }
}
