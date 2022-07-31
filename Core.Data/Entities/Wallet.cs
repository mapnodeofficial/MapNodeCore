using Core.Data.Entities;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeCoreApp.Data.Entities
{
    [Table("Wallets")]
    public class Wallet : DomainEntity<int>
    {
        public int TokenConfigId { get;set;}

        public decimal Amount { get;set;}

        public Guid AppUserId { get;set;}

        public DateTime CreatedOn { get;set;}

        public DateTime? UpdatedOn { get;set;}


        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        [ForeignKey("TokenConfigId")]
        public virtual TokenConfig TokenConfig { get; set; }
    }
}
