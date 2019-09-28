using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jafleet.Commons.EF;
using Microsoft.AspNetCore.Mvc;
using Noobow.Commons.EF.Twitter;

namespace NoobowNotifier.Controllers
{
    public class FollowsController : Controller
    {
        private readonly TwitterContext _tContext;
        public FollowsController(TwitterContext context)
        {
            _tContext = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail([FromQuery]string date)
        {
            var fv = _tContext.FollowsView.Where(f => f.EventDateString == date);
            
            return Json(fv);
        }

        public IActionResult Summary([FromQuery]string fromdate, [FromQuery]string todate)
        {
            var summary = _tContext.FollowsSummarys.Where(s => s.Date.CompareTo(fromdate) <= 0 && s.Date.CompareTo(todate) >= 0);

            return Json(summary);
        }
    }
}