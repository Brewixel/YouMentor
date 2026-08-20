using Application.Interfaces;

namespace Application.IntegrationTests.Infrastructure;

public class FakeCurrentUser : ICurrentUser
{
	public Guid? UserId { get; set; }
	public bool IsAuthenticated
		=> UserId.HasValue;
}
