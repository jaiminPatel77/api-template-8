//-----------------------------------------------------------------------------
// <copyright file="StaticFileServerStartupExtensions.cs" company="Neudesic">
// Copyright 2016 Neudesic, LLC
// </copyright>
//-----------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class StaticFileServerStartupExtensions
    {
        internal static void ConfigureStaticFiles(this Startup startup, IApplicationBuilder app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
