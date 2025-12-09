//-----------------------------------------------------------------------------
// <copyright file="AspNetMvcStartupExtensions.cs" company="Neudesic">
// Copyright 2016 Neudesic, LLC
// </copyright>
//-----------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class AspNetMvcStartupExtensions
    {
        internal static void ConfigureMvc(this Startup startup, IApplicationBuilder app)
        {
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});
            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapHub<ChatHub>("/chat");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

        internal static IMvcBuilder AddMvcServices(this Startup startup, IServiceCollection services)
        {

            return services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                options.SerializerSettings.ContractResolver =
              new CamelCasePropertyNamesContractResolver();
            });

            //return services.AddMvc();

            //services.AddMvc(options =>
            //{
            //    options.InputFormatters.OfType<JsonInputFormatter>().First().SupportedMediaTypes.Add(
            //        new MediaTypeHeaderValue("application/vnd.api+json"));
            //    options.OutputFormatters.OfType<JsonOutputFormatter>().First().SupportedMediaTypes.Add(
            //        new MediaTypeHeaderValue("application/vnd.api+json"));
            //});
        }
    }
}
