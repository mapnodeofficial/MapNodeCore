using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Enums
{
    public enum SavingRewardType
    {
        [Description("Interest Rate")]
        InterestRate = 1,
        [Description("Commission")]
        Commission = 2
    }
}
