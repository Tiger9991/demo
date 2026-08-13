using Application.Features.Traps.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Jobs
{
    public class ConnectivityCheckJob : IJob
    {
        private readonly ILogger<ConnectivityCheckJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ConnectivityCheckJob(ILogger<ConnectivityCheckJob> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Running connectivity check...");
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Get disconnected/inactive traps using the query handler logic
            var query = new GetActiveTrapsByGroupQuery(Status: "Inactive");
            var disconnected = await mediator.Send(query, context.CancellationToken);

            if (disconnected.Any())
            {
                _logger.LogWarning("{Count} traps are disconnected.", disconnected.Count);
            }
            else
            {
                _logger.LogInformation("All traps are connected. No disconnected traps found.");
            }
        }
    }
}
