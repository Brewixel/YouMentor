using Application.Interfaces;
using Application.Services;
using Application.Signals;
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

		services.AddScoped<INotificationSender, MassTransitNotificationSender>();
		services.AddScoped<IOutboxService, OutboxService>();
		services.AddSingleton<OutboxSignal>();

		return services;
	}
}
