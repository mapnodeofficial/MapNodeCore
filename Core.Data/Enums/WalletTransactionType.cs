using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Enums
{
    public enum WalletTransactionType
    {
        [Description("Deposit")]
        Deposit = 1,
        [Description("Withdraw")]
        Withdraw = 2,
        [Description("Buy Seed Round")]
        BuySeedRound = 5,
        [Description("Affiliate Seed Round")]
        AffiliateSeedRound = 6,
        [Description("Saving")]
        Saving = 7,
        [Description("Saving Referral Direct")]
        SavingReferralDirect = 8,
        [Description("Saving Profit")]
        SavingProfit = 9,
        [Description("Saving Profit on Referral")]
        SavingProfitOnReferral = 10,
        [Description("Unlock Block")]
        UnlockBlock = 11,
        [Description("Swap Hash To MNO")]
        SwapHashToMNO = 12,
        [Description("Purchase Seed Round")]
        PurchaseSeedRound = 13,
        [Description("Saving Leadership Commission")]
        SavingLeadershipCommission = 14,
        [Description("Transfer")]
        Transfer = 15,
        [Description("Swap MNO To USDT")]
        SwapMNOToUSDT = 16,
        [Description("Swap MNO To MNI")]
        SwapMNOToMNI = 17,
        [Description("Affiliate Swap MNO To MNI")]
        AffiliateSwapMNOToMNI = 18
    }
}
