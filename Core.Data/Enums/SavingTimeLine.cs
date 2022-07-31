using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Enums
{
    public enum SavingTimeLine
    {
        
        [Description("180 Days")]
        Day180 = 180,

        [Description("270 Days")]
        Day270 = 270,

        [Description("360 Days")]
        Day360 = 360,

        [Description("720 Days")]
        Day720 = 720
    }
}
