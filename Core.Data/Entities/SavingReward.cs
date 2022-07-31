using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("SavingRewards")]
    public class SavingReward : DomainEntity<int>
    {
        public Guid AppUserId { get; set; }

        public int SavingId { get; set; }

        public decimal InterestRate { get; set; }

        public decimal Amount { get; set; }

        public string Remarks { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid? ReferralId { get; set; }

        public SavingRewardType Type { get; set; }


        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        [ForeignKey("SavingId")]
        public virtual Saving Saving { get; set; }
    }
}
