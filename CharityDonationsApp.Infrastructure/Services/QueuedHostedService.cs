using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CharityDonationsApp.Infrastructure.Services;

public class QueuedHostedService(
    IBackgroundTaskQueue taskQueue,
    IServiceProvider serviceProvider,
    ILogger<QueuedHostedService> logger)
    : BackgroundService
{
    private static readonly string Separator = new('*', 120);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("""
                              {separator}
                              {queuedHostedService} is starting.
                              {separator}
                              """, Separator, nameof(QueuedHostedService), Separator);

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(stoppingToken);
            try
            {
                using var scope = serviceProvider.CreateScope();
                var sp = scope.ServiceProvider;
                await workItem(sp, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("""
                                {Separator} 
                                Error occurred executing background work item in {queuedHostedService}.

                                Exception Message: {Message}

                                Exception Type: {ExceptionType}

                                StackTrace: {StackTrace}
                                {Separator}

                                """, Separator, nameof(QueuedHostedService), ex.Message,
                    ex.GetType().FullName ?? ex.GetType().Name, ex.StackTrace, Separator);
            }
        }
    }
}