using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IAppDbContext
{
	DbSet<Session> Sessions { get; }
	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
