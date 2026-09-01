using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure;

public enum JobStatus { Pending, Processing, Done, Failed }

public class Job
{
	public long Id { get; set; }
	public string Type { get; set; } = null!;
	public string Payload { get; set; } = null!;
	public JobStatus Status { get; set; } = JobStatus.Pending;
	public DateTime RunAt { get; set; }
	public int Attempts { get; set; }
	public int MaxAttempts { get; set; } = 5;
	public string? LastError { get; set; }
	public DateTime CreatedAt { get; set; }
}
