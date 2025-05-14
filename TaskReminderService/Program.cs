using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskReminderService;
using TaskReminderService.Configuration;
using TaskReminderService.Data;
using TaskReminderService.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Register RabbitMQ settings from appsettings.json
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

        // Register AppDbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ??
                "Server=localhost;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"));

        // Register RabbitMQ service
        services.AddSingleton<IRabbitMqService, RabbitMqService>();

        // Register the Worker
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
