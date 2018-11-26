using Line.Messaging;
using Line.Messaging.Webhooks;
using NoobowNotifier.Constants;
using NoobowNotifier.Logics;
using NoobowNotifier.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoobowNotifier
{
    internal class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }

        public LineBotApp(LineMessagingClient lineMessagingClient)
        {
            this.messagingClient = lineMessagingClient;
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
                    replyMessage = new TextMessage(GALogics.GetReportStringMyNormal1());
                    break;
                case CommandConstant.JETPHOTS:
                    string reg = message[1];
                    (string photolarge, string photosmall)  = await JPLogics.GetJetPhotosFromRegistrationNumberAsync(reg);
                    replyMessage = new ImageMessage(photolarge, "https:" + photosmall);
                    break;
                case CommandConstant.PLAN_NOTICE:
                    doPush = DateTime.TryParseExact("20" + message[1], "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out pushTime);
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
                await messagingClient.PushMessageAsync(userId,new List<ISendMessage>{ new TextMessage(message[2]) });
            }

        }

        private string GetFileExtension(string mediaType)
        {
            switch (mediaType)
            {
                case "image/jpeg":
                    return ".jpeg";
                case "audio/x-m4a":
                    return ".m4a";
                case "video/mp4":
                    return ".mp4";
                default:
                    return "";
            }
        }
    }
}