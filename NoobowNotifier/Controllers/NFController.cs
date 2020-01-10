using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NoobowNotifier.Controllers
{
    public class NFController : Controller
    {
        static HttpClient hc = new HttpClient();
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SendAsync(string message)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "message", message }
            });

            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConfig.NFToken);
            var result = await hc.PostAsync("https://notify-api.line.me/api/notify", content);

            return Content("OK");
        }
    }
}