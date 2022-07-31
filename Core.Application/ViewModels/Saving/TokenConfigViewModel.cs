using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.System;
using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.Saving
{
    public class TokenConfigViewModel 
    {
        public TokenConfigViewModel()
        {
            TimeLines = new List<EnumModel>();
            TicketTransactions = new List<TicketTransactionViewModel>();
        }

        public int Id { get;set;}

        [Required]
        public string Name { get; set; }

        [Required]
        public string TokenCode { get; set; }

        [Required]
        public string ContractAddress { get; set; }

        [Required]
        public string TokenImageUrl { get; set; }

        [Required]
        [Range(0.1, 9999999)]
        public decimal TotalSupply { get; set; }

        [Required]
        [Range(1, 30)]
        public int Decimals { get; set; }
        public int ArrangeOrder { get; set; }

        [Required]
        [Range(0.1, 9999999)]
        public decimal MinSaving { get; set; }

        [Required]
        [Range(0.1, 9999999)]
        public decimal MaxSaving { get; set; }

        [Required]
        [Range(0.1, 9999999)]
        public decimal MinDeposit { get; set; }

        [Required]
        [Range(0.1, 9999999)]
        public decimal MaxDeposit { get; set; }

        [Required]
        [Range(0.1, 9999999)]
        public decimal MinWithdraw { get; set; }

        [Required]
        [Range(0.1, 9999999)]
        public decimal MaxWithdraw { get; set; }

        public decimal FeeWithdraw { get; set; }

        public decimal TotalSaving { get; set; }

        public int PerTotalSaving { get; set; }

        [Required]
        public TokenConfigType Type { get; set; }

        public string TypeName { get; set; }

        public decimal MaxInterestRate { get; set; }

        public DateTime? CreatedOn { get;set;}

        [Required] 
        public decimal Interest180Day { get; set; }

        [Required] 
        public decimal Interest270Day { get; set; }

        [Required]
        public decimal Interest360Day { get; set; }

        [Required]
        public decimal Interest720Day { get; set; }

        public List<EnumModel> TimeLines { get; set; }

        public string AbiConfig { get;set;}

        public decimal MinTransfer { get; set; }

        public decimal MinSwap { get; set; }

        public decimal TransferFee { get; set; }

        public decimal SwapFee { get; set; }

        public List<TicketTransactionViewModel> TicketTransactions { get; set; }
    }
}
