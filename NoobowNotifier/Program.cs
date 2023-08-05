using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CoreTweet;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noobow.Commons.Constants;
using Noobow.Commons.EF;
using Noobow.Commons.EF.Twitter;
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
            /*_services.AddLogging(builder => builder
                .AddConsole()
                .AddFilter(level => level >= LogLevel.Information)
            );*/
            //var loggerFactory = _services.BuildServiceProvider().GetService<ILoggerFactory>();
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();
            AppConfig.ToolsConnectionString = config.GetConnectionString("ToolsConnection");
            AppConfig.ToolsOption = new DbContextOptionsBuilder<ToolsContext>();
            AppConfig.ToolsOption/*.UseLoggerFactory(loggerFactory)*/.UseMySql(AppConfig.ToolsConnectionString, new MariaDbServerVersion(new Version(10, 4)));
            _services.AddDbContext<ToolsContext>(
                options => options/*.UseLoggerFactory(loggerFactory)*/.UseMySql(AppConfig.ToolsConnectionString, new MariaDbServerVersion(new Version(10, 4))
            ));

            _provider = _services.BuildServiceProvider();
            _context = _provider.GetService<ToolsContext>();

            AppConfig.NFToken = config.GetSection("NF")?["Token"];

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

        private static void RegisterNewUsers(IEnumerable<User> users, List<long> ids, TwitterContext context, FollowsTypeEnum followType, string owner)
        {
            foreach (long? id in ids)
            {
                var user = users.Where(u => u.Id == id).SingleOrDefault();
                if (user != null)
                {
                    var fu = context.FollowsUsers.Where(e => e.Owner == owner && e.UserId == id && e.FollowsType == followType).SingleOrDefault();
                    if (fu == null)
                    {
                        context.FollowsUsers.Add(new FollowsUser() { Owner = owner, UserId = user.Id, ScreenName = user.ScreenName, Name = user.Name, FollowsType = followType, Status = FollowsStatusEnum.Active });
                    }
                    else
                    {
                        fu.Status = FollowsStatusEnum.Active;
                        fu.Name = user.Name;
                        fu.ScreenName = user.ScreenName;
                    }
                }
            }
            context.SaveChanges();
        }

        private static void ChangeUserToInactive(List<long> ids, TwitterContext context, FollowsTypeEnum followType, string owner)
        {
            foreach (long? id in ids)
            {
                var fu = context.FollowsUsers.Where(e => e.Owner == owner && e.UserId == id && e.FollowsType == followType).SingleOrDefault();
                if (fu != null)
                {
                    fu.Status = FollowsStatusEnum.Inactive;
                }
            }
            context.SaveChanges();
        }

        private static void ProcessTargetUsers(IEnumerable<dynamic> users, List<long> ids, FollowsEventTypeEnum eventType, DateTime execDatetime, TwitterContext context)
        {
            foreach (var id in ids)
            {
                int followsId = users.Where(fu => fu.UserId == id).SingleOrDefault().FollowsId;
                int sameHistroyCount = context.FollowsEvents.Where(f => f.FollowsId == followsId && f.EventType == eventType).Count();
                context.FollowsEvents.Add(new FollowsEvent { EventDate = execDatetime, FollowsId = followsId, EventType = eventType, SameHistoryCount = sameHistroyCount });
            }
            context.SaveChanges();
        }

        private static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> list, int size)
        {
            while (list.Any())
            {
                yield return list.Take(size);
                list = list.Skip(size);
            }

        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseUrls("http://localhost:4000")
            .Build();
    }
}
