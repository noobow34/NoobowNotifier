using EnumStringValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoobowNotifier.Constants
{
    public enum CommandEum
    {
        Default,
        [StringValue("あ")]
        JAfleetGa,
        [StringValue("よ")]
        PlanNotice,
        [StringValue("より")]
        PlanList,
        [StringValue("えさ")]
        ExceptionDelete
    }
}
