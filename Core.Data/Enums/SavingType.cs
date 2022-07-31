using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Enums
{
    public enum SavingType
    {
        [Description("Active")]
        Active = 1,
        [Description("Cancel")]
        Cancel,
        [Description("Complete")]
        Complete
    }
}
