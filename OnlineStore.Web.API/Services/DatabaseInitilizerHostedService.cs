using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineStore.Data.Providers.Context;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OnlineStore.Web.API.Services
{
    public class DatabaseInitilizerHostedService : BackgroundService
    {
        private readonly ILogger<DatabaseInitilizerHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _environment;

        public DatabaseInitilizerHostedService(ILogger<DatabaseInitilizerHostedService> logger, IServiceScopeFactory scopeFactory, IWebHostEnvironment environment)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _environment = environment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Start initilizing database");

            if (_environment.IsDevelopment())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<OnlineStoreContext>();
                    await context.Database.EnsureCreatedAsync();
                }
            }
            else if (_environment.IsEnvironment("Development1"))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<OnlineStoreContext>();
                    await context.Database.MigrateAsync();
                }
            }

            _logger.LogInformation("Finish initializng database");
        }
    }
}
