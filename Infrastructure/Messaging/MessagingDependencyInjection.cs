using Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;

public static class MessagingDependencyInjection
{
	public static IServiceCollection AddMessaging(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddMassTransit(x =>
		{
			x.UsingRabbitMq((_, cfg) =>
			{
				cfg.Host(
					configuration["RabbitMq:Host"],
					configuration["RabbitMq:VirtualHost"],
					h =>
					{
						h.Username(configuration["RabbitMq:Username"]!);
						h.Password(configuration["RabbitMq:Password"]!);
					});
			});
		});

		services.AddScoped<
			IIntegrationEventPublisher,
			MassTransitIntegrationEventPublisher>();

		return services;
	}
}
