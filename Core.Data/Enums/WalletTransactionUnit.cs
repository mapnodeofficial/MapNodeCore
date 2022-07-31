using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Enums
{
    public enum WalletTransactionUnit
    {
        [Description("MNI")]
        MNI = 1,
        [Description("MNO")]
        MNO = 2,
        [Description("USDT")]
        USDT = 3,
        [Description("BNB")]
        BNB = 4
    }
}
