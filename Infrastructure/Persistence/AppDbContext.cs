using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities;
using Application.Interfaces;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<Session> Sessions { get; set; }
}
