using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Data.Entities
{
    [Table("SavingDefi")]
    public class SavingDefi : DomainEntity<Guid>
    {
        public string AddressTo { get; set; }
        public string AddressFrom { get; set; }
        public Guid AppUserId { get; set; }
        public SavingDefiTransactionState TransactionState { get; set; }
        public decimal USDAmount { get; set; }
        public decimal TokenAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string TokenTransactionHash { get; set; }
        public bool IsDevice { get; set; }

        public int Timeline { get;set;}
        public int TokenConfigId { get;set; }

        public string Remarks { get;set;}

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        [ForeignKey("TokenConfigId")]
        public virtual TokenConfig TokenConfig { set; get; }
    }
}
