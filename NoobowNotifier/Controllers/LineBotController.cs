using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using NoobowNotifier.Manager;

namespace NoobowNotifier.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LineBotController : Controller
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JToken req)
        { 
            var events = WebhookEventParser.Parse(req.ToString());

            var app = new LineBotApp(LineMessagingClientManager.GetInstance());
            await app.RunAsync(events);
            return new OkResult();
        }
    }
}
