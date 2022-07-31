using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using System;

namespace Core.Data.EF.Repositories
{

    public class SavingRewardRepository : EFRepository<SavingReward, int>, ISavingRewardRepository
    {
        public SavingRewardRepository(AppDbContext context) : base(context)
        {
        }

        public void AddSavingProfit(
            Guid userId,
            int savingId,
            decimal interestRate,
            decimal profit,
            SavingRewardType type)
        {
            Add(new SavingReward
            {
                Amount = profit,
                AppUserId = userId,
                CreatedOn = DateTime.Now,
                InterestRate = interestRate,
                SavingId = savingId,
                Type = type
            });
        }

        public void AddSavingProfit(
            Guid userId,
            int savingId,
            decimal interestRate,
            decimal profit,
            Guid? referralId,
            SavingRewardType type,
            string remarks)
        {
            Add(new SavingReward
            {
                Amount = profit,
                AppUserId = userId,
                CreatedOn = DateTime.Now,
                InterestRate = interestRate,
                SavingId = savingId,
                ReferralId = referralId,
                Remarks = remarks,
                Type = type
            });
        }
    }
}