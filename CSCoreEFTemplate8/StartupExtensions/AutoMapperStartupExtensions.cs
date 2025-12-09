using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSCoreEFTemplate8.ViewModels;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Services.AutoMapper;

namespace CSCoreEFTemplate8.StartupExtensions
{
    public static class AutoMapperStartupExtensions
    {
        internal static void AddAutoMapperServices(this Startup startup, IServiceCollection services)
        {

            IConfigurationProvider config =
                            new MapperConfiguration(cfg =>
                            {
                                ConfigureAutomapper.ConfigureDBModels(cfg);
                            });
            var mapper = config.CreateMapper();
            services.AddSingleton(config);
            services.AddSingleton(mapper);
        }
    }
}
