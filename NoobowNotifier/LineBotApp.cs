using jafleet.Commons.EF;
using Line.Messaging;
using Line.Messaging.Webhooks;
using NoobowNotifier.Constants;
using NoobowNotifier.Logics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoobowNotifier
{
    internal class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }
        private readonly jafleetContext _context;

        public LineBotApp(LineMessagingClient lineMessagingClient, jafleetContext context)
        {
            this.messagingClient = lineMessagingClient;
            _context = context;
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

            switch (message[0])
            {
                case CommandConstant.JAFLEET_GA:
                    replyMessage = new TextMessage(GALogics.GetReportStringMyNormal1(_context));
                    break;
                case CommandConstant.PLAN_NOTICE:
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
                default:
                    break;
            }

            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });

            if (doPush)
            {
                int sleepTime = (int)(pushTime - DateTime.Now).TotalMilliseconds;
                await Task.Delay(sleepTime);
                var quickReply = new QuickReply();
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("5分後", $"{CommandConstant.PLAN_NOTICE} a5 {message[2]}")));
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("10分後", $"{CommandConstant.PLAN_NOTICE} a10 {message[2]}")));
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("30分後", $"{CommandConstant.PLAN_NOTICE} a30 {message[2]}")));
                quickReply.Items.Add(new QuickReplyButtonObject(new MessageTemplateAction("1時間後", $"{CommandConstant.PLAN_NOTICE} a60 {message[2]}")));
                await messagingClient.PushMessageAsync(userId,new List<ISendMessage>{ new TextMessage(message[2],quickReply) });
            }
        }
    }
}