using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace Application.Tests;

public class IntegrationTestBase : IAsyncLifetime
{
	public string ConnectionString { get; set; } = null!;
	PostgreSqlContainer _dbContainer = null!;

	public async Task InitializeAsync()
	{
		_dbContainer = new PostgreSqlBuilder("postgres:18-alpine").Build();
		await _dbContainer.StartAsync();
		ConnectionString = _dbContainer.GetConnectionString();

		await using var context = BuildContext();
		await context.Database.MigrateAsync();
	}

	public async Task DisposeAsync()
	{
		await _dbContainer.StopAsync();
	}

	protected AppDbContext BuildContext(params IInterceptor[] interceptors)
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(ConnectionString)
			.AddInterceptors(interceptors)
			.Options;
		var context = new AppDbContext(options);
		return context;
	}

	protected async Task<Session?> GetSessionAsync(Guid id, bool ensureExists = true)
	{
		await using var context = BuildContext();
		var session = await context.Sessions.FindAsync(id);
		if (ensureExists && session == null)
			throw new InvalidOperationException($"Session with id = {id} not found");
		return session;
	}
}
