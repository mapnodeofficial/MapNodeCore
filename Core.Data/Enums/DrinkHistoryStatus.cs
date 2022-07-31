using System.ComponentModel;

namespace Core.Data.Enums
{
    public enum DrinkHistoryStatus
    {
        [Description("Active")]
        Active = 1, // new drink
        [Description("Stop Drink")]
        Inactive, // stop drink
        [Description("Processed")]
        Processed // process token calculate
    }
}
