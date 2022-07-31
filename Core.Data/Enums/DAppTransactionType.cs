using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum SavingDefiTransactionState
    {
        [Description("None")]
        None = 0,
        [Description("Requested")]
        Requested = 1,
        [Description("Confirmed")]
        Confirmed = 2,
        [Description("Rejected")]
        Rejected = 3,
        [Description("Failed")]
        Failed = 4,
    }

    public enum SaleDefiTransactionType
    {
        [Description("Seed Round")]
        SeedRound = 1,
        [Description("Angle Round")]
        AngleRound = 2,
        [Description("Private Round")]
        PrivateRound = 3,
        [Description("Publish Round")]
        PublishRound = 4
    }

    public enum SaleDefiTransactionState
    {
        [Description("None")]
        None = 0,
        [Description("Requested")]
        Requested = 1,
        [Description("Confirmed")]
        Confirmed = 2,
        [Description("Rejected")]
        Rejected = 3,
        [Description("Failed")]
        Failed = 4,
    }
}
