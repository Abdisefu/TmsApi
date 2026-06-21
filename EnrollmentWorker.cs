using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // Required for GetRequiredService
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TmsApi.Services;

namespace TmsApi.Workers
{
    public class EnrollmentWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EnrollmentWorker> _logger;

        // Constructor injecting the allowed IServiceScopeFactory
        public EnrollmentWorker(IServiceScopeFactory scopeFactory, ILogger<EnrollmentWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Call the batch processor method every hour
                await ProcessBatchAsync();

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        // The Task Method assigned by the material
        public async Task ProcessBatchAsync()
        {
            _logger.LogInformation("EnrollmentWorker processing batch at: {time}", DateTimeOffset.Now);

            // TODO2: Create a short-lived scope using the injected factory.
            using (var scope = _scopeFactory.CreateScope())
            {
                // TODO3: Resolve the scoped service from the new scope's provider.
                var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

                // TODO4: Use the service safely
                var enrollments = await enrollmentService.GetAllAsync();
                _logger.LogInformation("Successfully processed {Count} enrollments using an isolated scope.", enrollments.Count);
                
            } // The 'using' block ends here, disposing the scope and memory automatically!
        }
    }
}