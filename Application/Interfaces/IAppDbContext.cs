using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces;

public interface IAppDbContext
{
	DbSet<Session> Sessions { get; }
	DbSet<OutboxMessage> OutboxMessages { get; }

	ChangeTracker ChangeTracker { get; }
	DatabaseFacade Database { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
