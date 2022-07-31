using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.BlockChain
{
    public class WalletViewModel
    {
        public string Name { get; set; }
        public int TokenConfigId { get; set; }
        public string TokenCode { get; set; }
        public decimal Amount { get; set; }
        public string ContractAddress { get; set; }
        public string TokenImageUrl { get; set; }
        public decimal MinDeposit { get; set; }
        public decimal MaxDeposit { get; set; }
        public decimal MinWithdraw { get; set; }
        public decimal MaxWithdraw { get; set; }
        public decimal FeeWithdraw { get; set; }
    }
}
