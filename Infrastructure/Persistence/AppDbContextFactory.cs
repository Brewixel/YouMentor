using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var basePath = Directory.GetCurrentDirectory();

		var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
		                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
		                  ?? "Production";

		var configuration = new ConfigurationBuilder()
			.SetBasePath(basePath)
			.AddJsonFile("appsettings.json", optional: true)
			.AddJsonFile($"appsettings.{environment}.json", optional: true)
			.Build();

		var connectionString =
			Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
			?? configuration.GetConnectionString("DefaultConnection");

		var builder = new DbContextOptionsBuilder<AppDbContext>();

		if (string.IsNullOrEmpty(connectionString))
		{
			throw new InvalidOperationException($"Connection string 'DefaultConnection' not found. Searched in: {basePath} and ../Api/");
		}

		builder.UseNpgsql(connectionString)
			.UseSnakeCaseNamingConvention();

		return new AppDbContext(builder.Options);
	}
}
