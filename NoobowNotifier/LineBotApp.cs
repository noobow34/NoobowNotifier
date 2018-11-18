using Google.Apis.Analytics.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Line.Messaging;
using Line.Messaging.Webhooks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
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
            userMessage = userMessage.ToLower().Replace(" ", "");
            ISendMessage replyMessage = null;
            var certificate = new X509Certificate2(@"../keys/noobow.p12", "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer("noobow@noobow-222722.iam.gserviceaccount.com")
            {
                Scopes = new[] { AnalyticsService.Scope.Analytics, AnalyticsService.Scope.AnalyticsReadonly }
            }.FromCertificate(certificate));
            var service = new AnalyticsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "NoobowNotifier",
            });

            var todayString = DateTime.Now.ToString("yyyy-MM-dd");
            var yesterdayString = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            var userData = await service.Data.Ga.Get("ga:181171503", todayString, todayString,  "ga:users").ExecuteAsync();

            string userCount = "0";
            if(userData.Rows != null)
            {
                userCount = userData.Rows[0][0];
            }

            replyMessage = new TextMessage($"ç°ì˙: {userCount}");

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