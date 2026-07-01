namespace Application.Interfaces;

public interface ICurrentUser
{
	Guid? UserId { get; }          // null = not authenticated / no valid sub
	bool IsAuthenticated { get; }
}
