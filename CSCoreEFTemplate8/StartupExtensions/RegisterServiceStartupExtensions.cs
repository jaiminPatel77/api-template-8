using ConfidoSoft.Data.Services.BLServices;
using ConfidoSoft.Data.Services.Helpers;
using CSCoreEFTemplate8.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.StartupExtensions
{
    public static class RegisterServiceStartupExtensions
    {
        internal static void AddApplicationServices(this Startup startup, IServiceCollection services)
        {
            services.AddScoped<ICurrentUserInfo, CurrentUser>();
            services.AddScoped<ReCaptchaService>();            
            services.RegisterCommonServices();
        }
    }
}
