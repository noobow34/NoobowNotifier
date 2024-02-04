using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoobowNotifier.Models;
using NoobowNotifier.Middleware;
using jafleet.Commons.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using Noobow.Commons.EF;
using Microsoft.Extensions.Hosting;

namespace NoobowNotifier
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {           
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            services.AddDbContextPool<jafleetContext>(
                options => options/*.UseLoggerFactory(loggerFactory)*/.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
            );
            services.AddDbContextPool<ToolsContext>(
                options => options/*.UseLoggerFactory(loggerFactory)*/.UseNpgsql(Configuration.GetConnectionString("ToolsConnection"))
            );

            services.AddMvc().AddNewtonsoftJson();
            services.Configure<AppSettings>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseLineValidationMiddleware(Configuration.GetSection("LineSettings")["ChannelSecret"]);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default","{controller=Home}/{action=Index}");
            });
        }        
    }
}
