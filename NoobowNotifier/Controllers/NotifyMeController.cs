using Line.Messaging;
using Microsoft.AspNetCore.Mvc;
using Noobow.Commons.Constants;
using NoobowNotifier.Manager;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoobowNotifier.Controllers
{
    public class NotifyMeController : Controller
    {
        public async Task<IActionResult> Index([FromForm]string message)
        {
            await LineMessagingClientManager.GetInstance().PushMessageAsync(LineUserIdConstant.NOOBWO, new List<ISendMessage>() { new TextMessage(message) });
            return new OkResult();
        }
    }
}
