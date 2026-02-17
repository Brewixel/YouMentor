using Api.Endpoints;
using Application.Behaviors;
using Application.Core;
using Application.Interfaces;
using Application.Sessions;
using FluentValidation;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseNpgsql(connectionString));
		builder.Services.AddScoped<IAppDbContext>(provider =>
			provider.GetRequiredService<AppDbContext>());

		builder.Services.AddSingleton<SessionMapper>();

		builder.Services.AddOpenApi();

		builder.Services.AddValidatorsFromAssembly(typeof(Book).Assembly);
		builder.Services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly);
			cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
		});

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
			app.MapScalarApiReference();
		}

		app.MapSessionsEndpoints();

		app.Run();
	}
}
