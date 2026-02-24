using Microsoft.EntityFrameworkCore;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Entities;

namespace S2O1.API.Services
{
    public class SystemRestartService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SystemRestartService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private static bool _updateDetected = false;

        public SystemRestartService(IServiceProvider serviceProvider, ILogger<SystemRestartService> logger, IHostApplicationLifetime lifetime)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _lifetime = lifetime;
        }

        public static void MarkUpdateDetected() => _updateDetected = true;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("System Restart Monitor Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check every minute
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<S2O1DbContext>();

                    var restartTimeStr = (await context.SystemSettings
                        .FirstOrDefaultAsync(s => s.SettingKey == "AutoRestartTime"))?.SettingValue ?? "03:00";

                    if (TimeSpan.TryParse(restartTimeStr, out var restartTime))
                    {
                        var now = DateTime.Now.TimeOfDay;
                        
                        // If it's the scheduled minute and we have a reason to restart 
                        // (In a real scenario, we might check a flag or a file on disk indicating an update was pulled)
                        // For this request, we'll check if it's the exact minute.
                        if (now.Hours == restartTime.Hours && now.Minutes == restartTime.Minutes)
                        {
                            // Check if queue is empty (optional but recommended by user: "işlemleri bitir ve restart et")
                            var pendingTasks = await context.SystemQueueTasks.AnyAsync(t => t.Status == "Pending" || t.Status == "Processing");
                            if (pendingTasks)
                            {
                                _logger.LogInformation("Scheduled restart time reached, but tasks are still processing. Waiting for next check.");
                                continue;
                            }

                            _logger.LogInformation("Scheduled restart time reached ({restartTimeStr}). Initiating graceful shutdown.", restartTimeStr);
                            _lifetime.StopApplication();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SystemRestartService loop.");
                }
            }
        }
    }
}
