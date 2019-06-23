using EnumStringValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoobowNotifier.Constants
{
    public enum CommandEum
    {
        Other,
        [StringValue("あ")]
        JAfleetGa,
        [StringValue("よ")]
        PlanNotice,
        [StringValue("え"),StringValue("e")]
        Edit,
        [StringValue("より")]
        PlanList
    }
}
