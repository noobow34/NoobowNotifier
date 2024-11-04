using Microsoft.AspNetCore.Mvc;
using NoobowNotifier.Logics;

namespace NoobowNotifier.Controllers
{
    public class SwimRecordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string RecordSwim(string swimDate)
        {
            return GCLogics.RecordSwim(swimDate).ToString();
        }

        [HttpPost]
        public string CountSwim(string countTarget)
        {
            return GCLogics.CountSwim(countTarget).ToString();
        }
    }
}
