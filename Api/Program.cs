using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Infrastructure.Persistence;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseNpgsql(connectionString));

		builder.Services.AddOpenApi();
		builder.Services.AddMediatR(cfg => 
			cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly));

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
		}

		app.MapPost("/api/sessions", (IMediator mediator, CancellationToken ct) =>
		{
			mediator.Send();
		});

		app.Run();
	}
}