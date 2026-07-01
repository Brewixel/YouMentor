using System.Security.Claims;
using Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Api.Auth;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
	public Guid? UserId
	{
		get
		{
			var sub =  accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
			return Guid.TryParse(sub, out var id) ? id : null;
		}
	}

	public bool IsAuthenticated =>
		accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
