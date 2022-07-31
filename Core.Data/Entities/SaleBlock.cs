using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("SaleBlocks")]
    public class SaleBlock : DomainEntity<int>
    {
        public decimal Amount { get; set; }

        public string TransactionHash { get; set; }

        public SaleBlockType Type { get; set; }

        public SaleDefiTransactionType RoundType { get; set; }

        public Guid AppUserId { get; set; }

        public DateTime StartOn { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }


        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }
    }
}
