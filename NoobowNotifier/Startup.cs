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
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Noobow.Commons.EF;

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
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
            .AddConsole()
            .AddFilter(level => level >= LogLevel.Information)
            );
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();

            services.AddDbContextPool<jafleetContext>(
                options => options.UseLoggerFactory(loggerFactory).UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 3), ServerType.MariaDb);
                    }
            ));
            services.AddDbContextPool<ToolsContext>(
                options => options.UseLoggerFactory(loggerFactory).UseMySql(Configuration.GetConnectionString("ToolsConnection"),
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 3), ServerType.MariaDb);
                    }
            ));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2); ;
            services.Configure<AppSettings>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseLineValidationMiddleware(Configuration.GetSection("LineSettings")["ChannelSecret"]);
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                       name: "default",
                       template: "{controller=Home}/{action=Index}");
            });
        }        
    }
}
