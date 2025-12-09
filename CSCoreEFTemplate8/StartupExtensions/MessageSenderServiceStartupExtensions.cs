using ConfidoSoft.Data.Services.BLServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.StartupExtensions
{
    public static class MessageSenderServiceStartupExtensions
    {
        internal static void AddMessageSenderService(this Startup startup, IServiceCollection services)
        {
            services.AddScoped<IEmailSender, EmailMessageService>();
        }
    }
}
