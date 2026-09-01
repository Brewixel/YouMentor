using Microsoft.EntityFrameworkCore;
using NotificationService.Persistence.Entities;

namespace NotificationService.Persistence;

public class AppDbContext : DbContext
{
	public DbSet<Inbox> Inbox { get; set; }
	public DbSet<History> History { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
