using MassTransit;
using NotificationService.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SessionBookedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
	        builder.Configuration["RabbitMq:Host"],
	        builder.Configuration["RabbitMq:VirtualHost"],
	        h =>
	        {
	            h.Username(builder.Configuration["RabbitMq:Username"]!);
	            h.Password(builder.Configuration["RabbitMq:Password"]!);
	        });

        cfg.ReceiveEndpoint("notification-service", e =>
        {
            e.ConfigureConsumer<SessionBookedConsumer>(context);
        });
    });
});

var host = builder.Build();

host.Run();
