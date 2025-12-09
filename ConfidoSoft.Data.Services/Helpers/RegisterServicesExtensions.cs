using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Services.BLServices;
using ConfidoSoft.Data.Services.DBServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Services.Helpers
{
    /// <summary>
    /// Extension methods for IServiceCollection.
    /// To register common service type.
    /// </summary>
    public static class RegisterServicesExtensions
    {
        /// <summary>
        /// Register all services with appropriate interface with IServiceCollection.
        /// Which will be used by default DI framework.
        /// </summary>
        /// <param name="services">IServiceCollection to register all services</param>
        public static void RegisterCommonServices(this IServiceCollection services)
        {
            services.AddScoped<ISettingDataProtector, SettingDataProtector>();
            services.AddScoped<ApplicationDbContext>();            
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRefreshTokenService, UserRefreshTokenService>();
            services.AddScoped<ICurrentUserPermissionService, CurrentUserPermissionService>();
        }
    }
}
