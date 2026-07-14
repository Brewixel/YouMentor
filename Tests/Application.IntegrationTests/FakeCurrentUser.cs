using Application.Interfaces;

namespace Application.IntegrationTests;

public class FakeCurrentUser : ICurrentUser
{
	public Guid? UserId { get; set; }
	public bool IsAuthenticated
		=> UserId.HasValue;
}
