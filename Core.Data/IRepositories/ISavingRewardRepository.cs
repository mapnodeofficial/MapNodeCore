using Core.Data.Entities;
using Core.Data.Enums;
using Core.Infrastructure.Interfaces;
using System;

namespace Core.Data.IRepositories
{
    public interface ISavingRewardRepository : IRepository<SavingReward, int>
    {
        void AddSavingProfit(
            Guid userId,
            int savingId,
            decimal interestRate,
            decimal profit,
            SavingRewardType type);

        void AddSavingProfit(
            Guid userId,
            int savingId,
            decimal interestRate,
            decimal profit,
            Guid? referralId,
            SavingRewardType type,
            string remarks);
    }
}
