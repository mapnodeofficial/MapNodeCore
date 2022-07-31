using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("WalletTransfers")]
    public class WalletTransfer : DomainEntity<int>
    {
        public string PrivateKey { get; set; }
        public string PublishKey { get; set; }
        public string TransactionHash { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
