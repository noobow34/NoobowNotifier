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
                default:
                    break;
            }

            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });
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