using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Jobs
{
    public class BatteryUpdateJob : IJob
    {
        private readonly ILogger<BatteryUpdateJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BatteryUpdateJob(ILogger<BatteryUpdateJob> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hourly Battery Update Job started at {Time}", DateTime.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Fetch all active traps
            var activeTraps = await dbContext.Traps
                .Where(t => t.status == "Active")
                .ToListAsync(context.CancellationToken);

            if (activeTraps.Any())
            {
                // 2. Compute updated battery and indicator status using centralized domain logic
                foreach (var trap in activeTraps)
                {
                    trap.UpdateBattery();
                    trap.UpdateIndicatorStatus();
                }

                // 3. Save changes back to the database
                int rowsAffected = await dbContext.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation("Battery update job completed. Traps updated: {Count}", rowsAffected);
            }
            else
            {
                _logger.LogInformation("No active traps with remaining battery found for update.");
            }
        }
    }
}
