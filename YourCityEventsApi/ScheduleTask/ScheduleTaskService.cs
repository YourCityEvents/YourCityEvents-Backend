using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace YourCityEventsApi.ScheduleTask
{
    public class ScheduleTaskService:BackgroundService
    {
        public IServiceProvider Services { get; set; }
        
        public ScheduleTaskService(IServiceProvider services)
        {
            Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await SyncData(cancellationToken);
        }

        private async Task SyncData(CancellationToken cancellationToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();

                await scopedService.SyncData(cancellationToken);
            }
        }
        
    }
}