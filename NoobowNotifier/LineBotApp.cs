using Line.Messaging;
using Line.Messaging.Webhooks;
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
            userMessage = userMessage.ToLower().Replace(" ", "");
            ISendMessage replyMessage = null;

            replyMessage = new TextMessage($"You said: {userMessage}");

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