using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Application.Interfaces;

public interface IAppDbContext
{
	DbSet<Session> Sessions { get; }

	ChangeTracker ChangeTracker { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
