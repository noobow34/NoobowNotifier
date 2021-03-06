using EnumStringValues;
using jafleet.Commons.Constants;
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
using System.Text.RegularExpressions;
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
            CommandEum command;

            _ = message[0].TryParseStringValueToEnum<CommandEum>(out command);

            switch (command)
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
                    if(pushTime <= DateTime.Now)
                    {
                        doPush = false;
                    }
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
                    var tasks = _tContext.NotificationTasks.AsNoTracking().Where(t => t.NotificationTime >= DateTime.Now && t.NotificationTo == userId).OrderBy(t => t.NotificationTime).ToList();
                    var planList = new StringBuilder();
                    bool firstLine = true;
                    foreach (var t in tasks)
                    {
                        if (!firstLine)
                        {
                            planList.AppendLine();
                        }
                        else
                        {
                            firstLine = false;
                        }
                        planList.Append($"{t.NotificationTime?.ToString("yyyy/MM/dd HH:mm")}:{t.NotificationDetail} {t.Status.GetStringValue()}");
                    }
                    replyMessage = new TextMessage(planList.ToString());
                    break;
                case CommandEum.ExceptionDelete:
                    _context.Log.RemoveRange(_context.Log.Where(l => l.LogType == LogType.EXCEPTION));
                    int result = _context.SaveChanges();
                    replyMessage = new TextMessage($"{result}件削除しました");
                    break;
                default:
                    if (Regex.IsMatch(message[0], "[0-9]{4}") || Regex.IsMatch(message[0], "[0-9]{3}[a-zA-Z]{1}") || Regex.IsMatch(message[0], "[0-9]{2}[a-zA-Z]{2}"))
                    {
                        replyMessage = new TextMessage($"googlechromes://ja-fleet.noobow.me/e/JA{message[0].ToUpper()}");
                    }
                    break;
            }

            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });

            if (doPush)
            {
                //task登録
                var nTask = new NotificationTask { NotificationDetail = message[2], NotificationTime = pushTime, Status = NotificationTaskStatusEnum.INITIAL, NotificationTo = userId };
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