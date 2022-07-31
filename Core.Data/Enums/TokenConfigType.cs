using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Enums
{
    public enum TokenConfigType
    {
        [Description("Upcoming")]
        Upcoming = 0,
        [Description("Process")]
        Process = 1,
        [Description("Finish")]
        Finish = 2
    }
}
