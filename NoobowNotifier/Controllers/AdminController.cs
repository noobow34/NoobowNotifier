using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NoobowNotifier.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            string command = @"/home/noobow/test.sh";


            ProcessStartInfo psInfo = new ProcessStartInfo();

            psInfo.FileName = "/bin/sh";
            psInfo.Arguments = command;
            psInfo.UseShellExecute = false;
            psInfo.RedirectStandardOutput = true;

            Process p = Process.Start(psInfo);
            string output = p.StandardOutput.ReadToEnd();

            return Content(output);
        }
    }
}