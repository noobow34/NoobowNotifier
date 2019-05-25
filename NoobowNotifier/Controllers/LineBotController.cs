using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using NoobowNotifier.Manager;
using jafleet.Commons.EF;
using Line.Messaging;
using System.Collections.Generic;
using NoobowNotifier.Constants;
using Noobow.Commons.EF;
using Microsoft.Extensions.DependencyInjection;

namespace NoobowNotifier.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LineBotController : Controller
    {
        private readonly jafleetContext _context;
        private readonly ToolsContext _tContext;
        private readonly IServiceScopeFactory _services;
        public LineBotController(jafleetContext context, ToolsContext toolsContext, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _tContext = toolsContext;
            _services = serviceScopeFactory;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JToken req)
        { 
            var events = WebhookEventParser.Parse(req.ToString());

            var app = new LineBotApp(LineMessagingClientManager.GetInstance(),_context,_tContext,_services);
            await app.RunAsync(events);
            return new OkResult();
        }

    }
}
