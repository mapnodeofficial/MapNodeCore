using BeCoreApp.Data.Entities;
using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("TokenConfigs")]
    public class TokenConfig : DomainEntity<int>
    {
        public string Name { get; set; }

        public string TokenCode { get; set; }

        public string ContractAddress { get; set; }

        public string TokenImageUrl { get; set; }

        public decimal TotalSupply { get; set; }

        public int Decimals { get; set; }

        public int ArrangeOrder { get; set; }

        public decimal MinSaving { get; set; }

        public decimal MaxSaving { get; set; }

        public decimal MinDeposit { get; set; }

        public decimal MaxDeposit { get; set; }

        public decimal MinWithdraw { get; set; }

        public decimal MaxWithdraw { get; set; }

        public decimal FeeWithdraw { get; set; }

        public decimal TotalSaving { get; set; }

        public TokenConfigType Type { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public decimal Interest180Day { get; set; }

        public decimal Interest270Day { get; set; }

        public decimal Interest360Day { get; set; }

        public decimal Interest720Day { get; set; }

        public string AbiConfig { get; set; }

        public decimal MinTransfer { get; set; }

        public decimal MinSwap { get; set; }

        public decimal TransferFee { get; set; }

        public decimal SwapFee { get; set; }

        public virtual ICollection<Saving> Savings { set; get; }
        public virtual ICollection<SavingDefi> SavingDefis { set; get; }
        public virtual ICollection<Wallet> Wallets { set; get; }
        public virtual ICollection<TicketTransaction> TicketTransactions { set; get; }
    }
}
