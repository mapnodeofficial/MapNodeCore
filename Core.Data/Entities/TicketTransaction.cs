using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("TicketTransactions")]
    public class TicketTransaction : DomainEntity<int>
    {
        [Required]
        public string AddressFrom { get; set; }

        [Required]
        public string AddressTo { get; set; }

        [Required]
        public decimal Fee { get; set; }

        [Required]
        public decimal FeeAmount { get; set; }

        [Required]
        public decimal AmountReceive { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public TicketTransactionType Type { get; set; }

        [Required]
        public TicketTransactionStatus Status { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public DateTime DateUpdated { get; set; }

        [Required]
        public int TokenConfigId { get; set; }

        [Required]
        public Guid AppUserId { get; set; }


        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        [ForeignKey("TokenConfigId")]
        public virtual TokenConfig TokenConfig { set; get; }
    }
}
