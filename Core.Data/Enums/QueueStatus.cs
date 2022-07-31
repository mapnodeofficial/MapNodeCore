using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Enums
{
    public enum QueueStatus
    {
        [Description("Pending")]
        Pending = 0,
        [Description("In Progress")]
        Inprogress = 1,
        [Description("Failed")]
        Failed = 2,
        [Description("Successed")]
        Successed = 3
    }
}
