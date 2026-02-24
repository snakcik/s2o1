using Microsoft.EntityFrameworkCore;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Entities;
using System.Text.Json;

namespace S2O1.API.Services
{
    public class QueueProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueueProcessorService> _logger;

        public QueueProcessorService(IServiceProvider serviceProvider, ILogger<QueueProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queue Processor Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<S2O1DbContext>();

                    var tasks = await context.SystemQueueTasks
                        .Where(t => t.Status == "Pending" || t.Status == "Processing")
                        .OrderBy(t => t.CreateDate)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    foreach (var task in tasks)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        try
                        {
                            task.Status = "Processing";
                            await context.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Processing task {Id} of type {Type}", task.Id, task.TaskType);
                            
                            // Mock processing logic - extend this as needed
                            await ProcessTaskAsync(task, stoppingToken);

                            task.Status = "Completed";
                            task.ProcessedDate = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing task {Id}", task.Id);
                            task.Status = "Failed";
                            task.ErrorMessage = ex.Message;
                            task.RetryCount++;
                            if (task.RetryCount < 3) task.Status = "Pending"; // Retry
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in QueueProcessorService loop.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessTaskAsync(SystemQueueTask task, CancellationToken ct)
        {
            // Implementation for specific task types
            await Task.Delay(1000, ct); // Simulate work
        }
    }
}
