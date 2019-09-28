using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noobow.Commons.Constants;
using Noobow.Commons.EF;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace NoobowNotifier
{
    public class Program
    {
        private static IServiceCollection _services;
        private static IServiceProvider _provider;
        private static ToolsContext _context;
        private static System.Timers.Timer _timer;
        public static void Main(string[] args)
        {
            _services = new ServiceCollection();
            _services.AddLogging(builder => builder
                .AddConsole()
                .AddFilter(level => level >= LogLevel.Information)
            );
            var loggerFactory = _services.BuildServiceProvider().GetService<ILoggerFactory>();
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();
            AppConfig.ToolsConnectionString = config.GetConnectionString("ToolsConnection");
            AppConfig.ToolsOption = new DbContextOptionsBuilder<ToolsContext>();
            AppConfig.ToolsOption.UseLoggerFactory(loggerFactory).UseMySql(AppConfig.ToolsConnectionString,
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 3), ServerType.MariaDb);
                    }
            );
            _services.AddDbContext<ToolsContext>(
                options => options.UseLoggerFactory(loggerFactory).UseMySql(AppConfig.ToolsConnectionString,
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 3), ServerType.MariaDb);
                    }
            ));

            _provider = _services.BuildServiceProvider();
            _context = _provider.GetService<ToolsContext>();

            //実行中になっているタスクを再スタート
            DateTime nextEvent = DateTime.Now.AddMilliseconds(Convert.ToInt64(AppConfig.TimerInterval));
            var tasks = _context.NotificationTasks.AsNoTracking().Where(t => t.NotificationTime >= DateTime.Now && t.NotificationTime <= nextEvent && (t.Status == NotificationTaskStatusEnum.INITIAL || t.Status == NotificationTaskStatusEnum.EXECUTING)).ToList();
            foreach (var t in tasks)
            {
                int taskId = t.TaskId;
                Task.Run(() =>
                {
                    var executer = new TaskExecuter(taskId);
                    _= executer.ExecuteAsync();
                });
            }

            //タイマースタート
            _timer = new System.Timers.Timer(AppConfig.TimerInterval);
            _timer.Elapsed += new ElapsedEventHandler(ReadAndExecuteTask);
            _timer.Start();

            BuildWebHost(args).Run();
        }

        private static void ReadAndExecuteTask(object sender, EventArgs e)
        {
            DateTime nextEvent = DateTime.Now.AddMilliseconds(Convert.ToInt64(AppConfig.TimerInterval));
            var tasks = _context.NotificationTasks.AsNoTracking().Where(t => t.NotificationTime >= DateTime.Now && t.NotificationTime <= nextEvent && t.Status == NotificationTaskStatusEnum.INITIAL).ToList();
            foreach (var t in tasks)
            {
                int taskId = t.TaskId;
                Task.Run(() =>
                {
                    var executer = new TaskExecuter(taskId);
                    _= executer.ExecuteAsync();
                });
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseUrls("http://localhost:4000")
            .Build();
    }
}
