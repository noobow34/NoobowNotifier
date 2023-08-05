using Microsoft.EntityFrameworkCore;
using Noobow.Commons.EF;
using Noobow.Commons.EF.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoobowNotifier
{
    public static class AppConfig
    {
        public static string ToolsConnectionString { get; set; }
        public const int TimerInterval = 60 * 60 * 1000;
        public static DbContextOptionsBuilder<ToolsContext> ToolsOption { get; set; }
        public static string NFToken { get; set; }
    }
}
