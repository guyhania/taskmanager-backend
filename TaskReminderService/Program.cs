using Microsoft.EntityFrameworkCore;
using TaskReminderService;
using TaskReminderService.Data;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Register AppDbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer("Server=localhost;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"));

        // Register the Worker
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
