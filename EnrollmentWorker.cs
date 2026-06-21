using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TmsApi.Services;

namespace TmsApi.Workers
{
    // Buggy implementation: Directly capturing a Scoped service in a Singleton background worker
    public class EnrollmentWorker : BackgroundService
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<EnrollmentWorker> _logger;

        // Constructor directly accepts the Scoped service, causing the captive dependency bug
        public EnrollmentWorker(IEnrollmentService enrollmentService, ILogger<EnrollmentWorker> logger)
        {
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("EnrollmentWorker running scholarship calculations at: {time}", DateTimeOffset.Now);
                
                // Simulating background processing using the captured service
                var enrollments = await _enrollmentService.GetAllAsync();
                _logger.LogInformation("Processed {Count} enrollments for scholarship evaluation.", enrollments.Count);

                // Wait 1 hour as required by the context
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}