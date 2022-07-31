using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum TicketTransactionUnit
    {
        [Description("MNI")]
        Token = 1,
        [Description("USDT")]
        USDT = 2,
        [Description("BNB")]
        BNB = 3
    }
}
