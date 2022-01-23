using Line.Messaging;
using Microsoft.AspNetCore.Mvc;
using Noobow.Commons.Constants;
using NoobowNotifier.Manager;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoobowNotifier.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CallAsync(string latitude, string longitude)
        {
            string sedMessage = $"https://maps.google.co.jp/maps?q={latitude},{longitude}";
            await LineMessagingClientManager.GetInstance().PushMessageAsync(LineUserIdConstant.NOOBWO, new List<ISendMessage>() { new TextMessage("ヘルプボタンが押されました") });
            await LineMessagingClientManager.GetInstance().PushMessageAsync(LineUserIdConstant.NOOBWO, new List<ISendMessage>() { new TextMessage(sedMessage) });
            await LineMessagingClientManager.GetInstance().PushMessageAsync(LineUserIdConstant.HARUKAINANA, new List<ISendMessage>() { new TextMessage("ヘルプボタンが押されました") });
            await LineMessagingClientManager.GetInstance().PushMessageAsync(LineUserIdConstant.HARUKAINANA, new List<ISendMessage>() { new TextMessage(sedMessage) });
            return Content("OK");
        }
    }
}
