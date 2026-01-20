using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var basePath = Directory.GetCurrentDirectory();

		var configuration = new ConfigurationBuilder()
			.SetBasePath(basePath)
			.AddJsonFile("appsettings.json", optional: true)
			.AddJsonFile(Path.Combine(basePath, "../Api/appsettings.json"), optional: true)
			.Build();

		var builder = new DbContextOptionsBuilder<AppDbContext>();

		var connectionString = configuration.GetConnectionString("DefaultConnection");

		if (string.IsNullOrEmpty(connectionString))
		{
			throw new InvalidOperationException($"Connection string 'DefaultConnection' not found. Searched in: {basePath} and ../Api/");
		}

		builder.UseNpgsql(connectionString);

		return new AppDbContext(builder.Options);
	}
}
