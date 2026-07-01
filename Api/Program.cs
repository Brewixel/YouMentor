using Api.Auth;
using Api.Endpoints;
using Api.ExceptionHandling;
using Api.Middlewares;
using Application.Behaviors;
using Application.Core;
using Application.Interfaces;
using Application.Sessions;
using FluentValidation;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseNpgsql(connectionString)
				.UseSnakeCaseNamingConvention());
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

		builder.Services.AddProblemDetails();
		builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

		builder.Services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.Authority = builder.Configuration["Auth:Authority"];
				options.Audience = builder.Configuration["Auth:Audience"];

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ClockSkew = TimeSpan.FromSeconds(30),
					RoleClaimType = "roles",
				};

				options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
				options.MapInboundClaims = false;
		});

		builder.Services.AddAuthorization(options =>
		{
			options.AddPolicy(Policies.MentorOnly, p => p.RequireRole("mentor"));
			options.AddPolicy(Policies.StudentOnly, p => p.RequireRole("student"));
		});
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddScoped<ICurrentUser, CurrentUser>();

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseRequestTiming();
		}

		app.UseExceptionHandler();

		app.UseAuthentication();
		app.UseAuthorization();

		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
			app.MapScalarApiReference();
		}

		app.MapSessionsEndpoints();

		app.Run();
	}
}
