using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using NoobowNotifier.Manager;
using jafleet.Commons.EF;
using Line.Messaging;
using System.Collections.Generic;

namespace NoobowNotifier.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LineBotController : Controller
    {
        private readonly jafleetContext _context;
        public LineBotController(jafleetContext context)
        {
            _context = context;
        }

        public string NOOBWO_USER_ID { get; private set; }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JToken req)
        { 
            var events = WebhookEventParser.Parse(req.ToString());

            var app = new LineBotApp(LineMessagingClientManager.GetInstance(),_context);
            await app.RunAsync(events);
            return new OkResult();
        }

        /// <summary>
        /// POST: api/NotifyMe
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]string message)
        {
            await LineMessagingClientManager.GetInstance().PushMessageAsync(NOOBWO_USER_ID, new List<ISendMessage>() { new TextMessage(message) });

            return new OkResult();
        }
    }
}
