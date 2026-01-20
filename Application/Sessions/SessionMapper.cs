using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Sessions;

[Mapper]
public partial class SessionMapper
{
	public partial SessionDto SessionToDto(Session session);
	public partial List<SessionDto> SessionToDto(List<Session> sessions);
}
