using EnumStringValues;
using Line.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Noobow.Commons.Constants;
using Noobow.Commons.EF;
using NoobowNotifier.Constants;
using NoobowNotifier.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NoobowNotifier
{
    public class TaskExecuter
    {
        public int TaskId { get; private set; }

        public TaskExecuter(int taskId)
        {
            TaskId = taskId;
        }

        public async Task ExecuteAsync()
        {
            using (var context = new ToolsContext(AppConfig.ToolsOption.Options))
            {
                //実行中に更新
                var task = context.NotificationTasks.Where(t => t.TaskId == this.TaskId).SingleOrDefault();
                task.Status = Noobow.Commons.Constants.NotificationTaskStatusEnum.EXECUTING;
                context.SaveChanges();
                //待ち時間を計算
                int waitTime = Convert.ToInt32((task.NotificationTime - DateTime.Now).Value.TotalMilliseconds);
                //待つ
                Thread.Sleep(waitTime);
                //通知
                var quickReply = new QuickReply();
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("5分後", $"{CommandEum.PlanNotice.GetStringValue()} a5 {task.NotificationDetail}")));
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("10分後", $"{CommandEum.PlanNotice.GetStringValue()} a10 {task.NotificationDetail}")));
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("30分後", $"{CommandEum.PlanNotice.GetStringValue()} a30 {task.NotificationDetail}")));
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("1時間後", $"{CommandEum.PlanNotice.GetStringValue()} a60 {task.NotificationDetail}")));
                await LineMessagingClientManager.GetInstance().PushMessageAsync(task.NotificationTo, new List<ISendMessage> { new TextMessage(task.NotificationDetail, quickReply) });
                //実行済みに更新
                task.Status = NotificationTaskStatusEnum.EXECUTED;
                context.SaveChanges();
            }
        }
    }
}
