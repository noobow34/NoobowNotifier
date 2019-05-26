using EnumStringValues;
using jafleet.Commons.EF;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noobow.Commons.Constants;
using Noobow.Commons.EF;
using Noobow.Commons.EF.Tools;
using NoobowNotifier.Constants;
using NoobowNotifier.Logics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoobowNotifier
{
    internal class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }
        private readonly jafleetContext _context;
        private readonly ToolsContext _tContext;
        private readonly IServiceScopeFactory _services;

        public LineBotApp(LineMessagingClient lineMessagingClient, jafleetContext context,ToolsContext toolsContext, IServiceScopeFactory serviceScopeFactory)
        {
            this.messagingClient = lineMessagingClient;
            _context = context;
            _tContext = toolsContext;
            _services = serviceScopeFactory;
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message.Type)
            {
                case EventMessageType.Text:
                    await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.UserId);
                    break;
            }
        }

        private async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            var message = userMessage.Split(" ");
            DateTime pushTime = DateTime.Now;
            bool doPush = false;
            ISendMessage replyMessage = null;

            switch (message[0].ParseToEnum<CommandEum>())
            {
                case CommandEum.JAfleetGa:
                    replyMessage = new TextMessage(GALogics.GetReportStringMyNormal1(_context));
                    break;
                case CommandEum.PlanNotice:
                    string additional = string.Empty;
                    if (message[1].StartsWith("a"))
                    {
                        message[1] = DateTime.Now.AddMinutes(Int32.Parse(message[1].Replace("a", string.Empty))).ToString("yyyyMMddHHmm");
                    }else if(message[1].Length == 4)
                    {
                        additional = DateTime.Now.ToString("yyyyMMdd");
                    }
                    else if(message[1].Length == 8)
                    {
                        additional = DateTime.Now.ToString("yyyy");
                    }else if(message[1].Length == 10)
                    {
                        additional = "20";
                    }

                    doPush = DateTime.TryParseExact(additional + message[1], "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out pushTime);
                    string mes;
                    if (doPush) {
                        mes = "予約しました";
                    }
                    else
                    {
                        mes = "予約に失敗しました";
                    }
                    replyMessage = new TextMessage(mes);
                    break;
                case CommandEum.PlanList:
                    var tasks = _tContext.NotificationTasks.AsNoTracking().Where(t => t.NotificationTime >= DateTime.Now).ToList();
                    var planList = new StringBuilder();
                    foreach (var t in tasks)
                    {
                        planList.Append($"{t.NotificationTime?.ToString("yyyy/MM/dd HH:mm")}:{t.NotificationDetail}:{t.Status.GetStringValue()}\n");
                    }
                    replyMessage = new TextMessage(planList.ToString());
                    break;
                default:
                    break;
            }

            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });

            if (doPush)
            {
                //task登録
                var nTask = new NotificationTask { NotificationDetail = message[2], NotificationTime = pushTime, Status = NotificationTaskStatusEnum.INITIAL };
                _tContext.NotificationTasks.Add(nTask);
                _tContext.SaveChanges();
                
                if((pushTime - DateTime.Now).TotalMilliseconds < AppConfig.TimerInterval)
                {
                    //1時間以内なら即座にタスクを実行
                    //1時間以上ならタイマーに任せる
                    _ = Task.Run(() =>
                    {
                        using (var serviceScope = _services.CreateScope())
                        {
                            var executer = new TaskExecuter(nTask.TaskId);
                            _ = executer.ExecuteAsync();
                        }
                    });
                }
            }
        }
    }
}