using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
//using Neuron.Healthmonitor.Service.Hub;
//using RecurrentTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class SignalRStartupExtensions
    {
        internal static void AddSignalRServices(this Startup startup, IServiceCollection services)
        {
            //services.AddSignalR()
            //   .AddNewtonsoftJsonProtocol();
        }

        internal static void ConfigureSignalR(this Startup startup, IApplicationBuilder app)
        {
            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<HealthMonitorHub>("SignalR");
            //});
            //app.StartTask<HealthMonitorHubClientService>(TimeSpan.FromSeconds(5));
        }
    }
}
