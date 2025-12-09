//-----------------------------------------------------------------------------
// <copyright file="EntityFrameworkStartupExtensions.cs" company="Neudesic">
// Copyright 2016 Neudesic, LLC
// </copyright>
//-----------------------------------------------------------------------------

using ConfidoSoft.Data.Domain.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class EntityFrameworkStartupExtensions
    {
        internal static void AddEntityFrameworkServices(this Startup startup, IServiceCollection services)
        {
            var connectionString = startup.Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        }
    }
}
