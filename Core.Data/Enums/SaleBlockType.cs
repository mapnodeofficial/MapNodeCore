using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum SaleBlockType
    {
        [Description("Pending")]
        Pending = 0,
        [Description("Successed")]
        Successed = 1
    }
}
