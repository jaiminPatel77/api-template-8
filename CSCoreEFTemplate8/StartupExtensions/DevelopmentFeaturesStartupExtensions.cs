//-----------------------------------------------------------------------------
// <copyright file="DevelopmentFeaturesStartupExtensions.cs" company="Neudesic">
// Copyright 2016 Neudesic, LLC
// </copyright>
//-----------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class DevelopmentFeaturesStartupExtensions
    {
        internal static void ConfigureDevelopmentFeatures(this Startup startup, IHostEnvironment env, IApplicationBuilder app)
        {
            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //Default exception handler or find better way!..need to see more detail.
                app.UseExceptionHandler(
                 builder =>
                 {
                     builder.Run(
                                async context =>
                             {
                                 context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                 context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                                 var error = context.Features.Get<IExceptionHandlerFeature>();
                                 if (error != null)
                                 {
                                     context.Response.Headers.Add("Application-Error", error.Error.Message);
                                     // CORS
                                     context.Response.Headers.Add("access-control-expose-headers", "Application-Error");
                                     await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                                 }
                             });
                 });
                //Or use the error page???
                //app.UseExceptionHandler("/Home/Error");
            }
        }
    }
}
