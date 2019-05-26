using EnumStringValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoobowNotifier.Constants
{
    public enum CommandEum
    {
        /*public const string JAFLEET_GA = "あ";
        public const string PLAN_NOTICE = "よ"*/
        [StringValue("あ")]
        JAfleetGa,
        [StringValue("よ")]
        PlanNotice,
        [StringValue("より")]
        PlanList
    }
}
