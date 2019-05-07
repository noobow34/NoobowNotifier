using Line.Messaging;
using Microsoft.AspNetCore.Mvc;
using Noobow.Commons.Constants;
using NoobowNotifier.Manager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoobowNotifier.Controllers
{
    public class NotifyMeController : Controller
    {
        public async Task<IActionResult> Index([FromForm]string message,string text)
        {
            string sendMessage = message ?? text ?? "空通知";
            try
            {
                await LineMessagingClientManager.GetInstance().PushMessageAsync(LineUserIdConstant.NOOBWO, new List<ISendMessage>() { new TextMessage(sendMessage) });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new OkResult();
        }
    }
}
