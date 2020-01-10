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
        private static TwitterContext _twContext;
        private static System.Timers.Timer _timer;
        private static System.Timers.Timer _twitterTimer;
        private static IConfigurationSection _twitterSettings;
        private static Tokens _tokens;
        static readonly string[] TargetScreenNames = new string[] { "noobow", "ja_fleet" };
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

            AppConfig.TwitterConnectionString = config.GetConnectionString("TwitterConnection");
            AppConfig.TwitterOption = new DbContextOptionsBuilder<TwitterContext>();
            AppConfig.TwitterOption.UseLoggerFactory(loggerFactory).UseMySql(AppConfig.ToolsConnectionString,
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 3), ServerType.MariaDb);
                    }
            );
            _services.AddDbContext<TwitterContext>(
                options => options.UseLoggerFactory(loggerFactory).UseMySql(AppConfig.TwitterConnectionString,
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 3), ServerType.MariaDb);
                    }
            ));

            _provider = _services.BuildServiceProvider();
            _context = _provider.GetService<ToolsContext>();
            _twContext = _provider.GetService<TwitterContext>();

            _twitterSettings = config.GetSection("TwitterSettings");
            _tokens = Tokens.Create(_twitterSettings["APIKey"], _twitterSettings["APISecret"], _twitterSettings["AccessKey"], _twitterSettings["AccessSecret"]);

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

            //Twitterタスクタイマースタート
            _twitterTimer = new System.Timers.Timer(AppConfig.TwitterTimerInterVal);
            _twitterTimer.Elapsed += new ElapsedEventHandler(RecordMyFollows);
            _twitterTimer.Start();

            BuildWebHost(args).Run();
        }

        private static void RecordMyFollows(object sender, EventArgs e)
        {
            var execDatetime = DateTime.Now;

            var optionsBuilder = new DbContextOptionsBuilder<TwitterContext>();
            foreach (string sn in TargetScreenNames)
            {
                Console.WriteLine($"処理ユーザー：{sn}");
                //今現在のリストをAPIから取得
                var currentFollowerIds = _tokens.Followers.EnumerateIds(EnumerateMode.Next, screen_name => sn, count => 5000).ToList();
                var currentFollowingIds = _tokens.Friends.EnumerateIds(EnumerateMode.Next, screen_name => sn, count => 200).ToList();

                Console.WriteLine($"現在のFollower：{currentFollowerIds.Count}");
                Console.WriteLine($"現在のFollowing：{currentFollowingIds.Count}");

                //前回実行時のリストをDBから取得
                var prevFollowerUsers = _twContext.FollowsUsers.Where(fu => fu.Owner == sn && fu.FollowsType == FollowsTypeEnum.Follower && fu.Status == FollowsStatusEnum.Active).Select(fu => new { FollowsId = fu.FollowsId, UserId = fu.UserId }).ToList();
                var prevFollowingUsers = _twContext.FollowsUsers.Where(fu => fu.Owner == sn && fu.FollowsType == FollowsTypeEnum.Following && fu.Status == FollowsStatusEnum.Active).Select(fu => new { FollowsId = fu.FollowsId, UserId = fu.UserId }).ToList();
                var prevFollowerUserIds = prevFollowerUsers.Select(u => u.UserId.Value).ToList();
                var prevFollowingUserIds = prevFollowingUsers.Select(u => u.UserId.Value).ToList();

                //比較
                //Follower
                var increaseFollowerIds = currentFollowerIds.Except(prevFollowerUserIds).ToList(); //今-前：自分をフォローした人
                var decreaseFollowerIds = prevFollowerUserIds.Except(currentFollowerIds).ToList(); //前-今：自分をフォロー解除した人
                //Following
                var increaseFollowingIds = currentFollowingIds.Except(prevFollowingUserIds).ToList(); //今-前：自分がフォローした人
                var decreaseFollowingIds = prevFollowingUserIds.Except(currentFollowingIds).ToList(); //前-今：自分がフォロー解除した人

                Console.WriteLine($"増加Follower：{increaseFollowerIds.Count}");
                Console.WriteLine($"減少Follower：{decreaseFollowerIds.Count}");
                Console.WriteLine($"増加Following：{increaseFollowingIds.Count}");
                Console.WriteLine($"減少Following：{decreaseFollowingIds.Count}");

                var increaseUsersIds = new List<long>();
                increaseUsersIds.AddRange(increaseFollowerIds);
                increaseUsersIds.AddRange(increaseFollowingIds);

                var increaseUsers = new List<User>();
                foreach (var chunked in Chunk<long>(increaseUsersIds, 100))
                {
                    increaseUsers.AddRange(_tokens.Users.Lookup(increaseUsersIds).ToList());
                }

                //新参者を登録
                RegisterNewUsers(increaseUsers, increaseFollowerIds, _twContext, FollowsTypeEnum.Follower, sn);
                RegisterNewUsers(increaseUsers, increaseFollowingIds, _twContext, FollowsTypeEnum.Following, sn);
                //消えた人のステータスを変更
                ChangeUserToInactive(decreaseFollowerIds, _twContext, FollowsTypeEnum.Follower, sn);
                ChangeUserToInactive(decreaseFollowingIds, _twContext, FollowsTypeEnum.Following, sn);

                //DB再読込
                var currentRegisteredFollowerUsers = _twContext.FollowsUsers.Where(fu => fu.Owner == sn && fu.FollowsType == FollowsTypeEnum.Follower).Select(fu => new { FollowsId = fu.FollowsId, UserId = fu.UserId }).ToList();
                var currentRegisteredFollowingUsers = _twContext.FollowsUsers.Where(fu => fu.Owner == sn && fu.FollowsType == FollowsTypeEnum.Following).Select(fu => new { FollowsId = fu.FollowsId, UserId = fu.UserId }).ToList();

                //比較結果をイベント登録
                ProcessTargetUsers(currentRegisteredFollowerUsers, increaseFollowerIds, FollowsEventTypeEnum.Do, execDatetime, _twContext);
                ProcessTargetUsers(currentRegisteredFollowerUsers, decreaseFollowerIds, FollowsEventTypeEnum.Un, execDatetime, _twContext);
                ProcessTargetUsers(currentRegisteredFollowingUsers, increaseFollowingIds, FollowsEventTypeEnum.Do, execDatetime, _twContext);
                ProcessTargetUsers(currentRegisteredFollowingUsers, decreaseFollowingIds, FollowsEventTypeEnum.Un, execDatetime, _twContext);

                Console.WriteLine("---------------------------");
            }
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
