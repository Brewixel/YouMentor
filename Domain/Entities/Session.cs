using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
	public class Session
	{
		public Guid Id { get; private set; }

		public Guid MentorId { get; private set; }
		public Guid? StudentId { get; private set; }

		public DateTime StartTime { get; private set; }
		public TimeSpan Duration { get; private set; }

		public SessionStatus Status { get; private set; }

		public Session(Guid mentorId, DateTime startTime, TimeSpan duration)
		{
			if (startTime < DateTime.UtcNow)
				throw new ArgumentException("You cannot create a session in the past.");

			Id = Guid.NewGuid();
			MentorId = mentorId;
			StartTime = startTime;
			Duration = duration;
			Status = SessionStatus.Free;
		}

		protected Session() { }

		public void Book(Guid studentId)
		{
			if (Status != SessionStatus.Free)
				throw new InvalidOperationException("This slot is already taken or cancelled.");

			if (studentId == Guid.Empty)
				throw new ArgumentException("Incorrect student Id.");

			StudentId = studentId;
			Status = SessionStatus.Booked;
		}
	}
}
