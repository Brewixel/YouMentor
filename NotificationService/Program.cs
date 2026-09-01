using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consumers;
using NotificationService.Persistence;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendNotificationConsumer>();

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
            e.ConfigureConsumer<SendNotificationConsumer>(context);
        });
    });
});

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

var host = builder.Build();

host.Run();
