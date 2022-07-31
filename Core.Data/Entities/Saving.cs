using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("Savings")]
    public class Saving : DomainEntity<int>
    {
        public SavingTimeLine Timeline { get;set;}

        public decimal AmountSaving { get;set;}


        public SavingType Type { get;set;}

        public int TimesReceived { get; set; }

        public DateTime StartOn { get; set; }

        public DateTime? CompletionOn { get; set; }

        public DateTime? LastReceived { get;set;}

        public DateTime CreatedOn { get; set; }

        public Guid AppUserId { get; set; }

        public int TokenConfigId { get; set; }

        public string Remarks { get;set;}

        public Guid TransactionId { get;set;}

        public decimal AmountUSD { get;set;}

        public DateTime? CancelOn { get; set; }

        public int ReceivedCount { get;set;}

        public int RemainCount { get;set;}

        public decimal InterestedRate { get; set; }

        public decimal ExpectedInterested { get; set; }


        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        [ForeignKey("TokenConfigId")]
        public virtual TokenConfig TokenConfig { get; set; }

        public virtual ICollection<SavingReward> SavingRewards { set; get; }
    }
}
