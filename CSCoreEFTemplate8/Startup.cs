using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSCoreEFTemplate8.StartupExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSCoreEFTemplate8
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //
            this.AddMessageSenderService(services);
            this.AddSignalRServices(services);
            this.AddMvcServices(services);

            this.AddEntityFrameworkServices(services);
            this.AddAspNetIdentityServices(services);

            this.AddSwaggerServices(services);
            this.AddCorsServices(services);
            this.AddAutoMapperServices(services);
            
            this.AddApplicationServices(services);

            this.AddAuthServices(services);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            this.ConfigureLogging(loggerFactory);
            this.ConfigureCors(app);
            this.ConfigureDevelopmentFeatures(env, app);
            this.ConfigureStaticFiles(app);
            app.UseRouting();
            //this.ConfigureSignalR(app);
            this.ConfigureIdentity(app);
            this.ConfigureMvc(app);
            this.ConfigureSwagger(app);
            this.InitializeDatabase(app);

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
