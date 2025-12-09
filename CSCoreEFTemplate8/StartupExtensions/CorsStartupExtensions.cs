//-----------------------------------------------------------------------------
// <copyright file="CorsStartupExtensions.cs" company="Neudesic">
// Copyright 2016 Neudesic, LLC
// </copyright>
//-----------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class CorsStartupExtensions
    {
        internal static void ConfigureCors(this Startup startup, IApplicationBuilder app)
        {
            app.UseCors("CORSP");
        }

        internal static void AddCorsServices(this Startup startup, IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("CORSP", builder =>
            {
                builder.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
            }));
        }
    }
}
