using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.Dapp
{
    public class DAppTransactionView
    {
        public Guid Id { get; set; }
        public string AddressTo { get; set; }
        public string AddressFrom { get; set; }
        public Guid? AppUserId { get; set; }
        public string Email { get; set; }
        public SavingDefiTransactionState TransactionState { get; set; }
        public string DAppTransactionStateName { get; set; }
        public string TypeName { get; set; }
        public decimal BNBAmount { get; set; }
        public decimal TokenAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string BNBTransactionHash { get; set; }
        public string TokenTransactionHash { get; set; }
        public bool IsDevice { get; set; }
        public string WalletType { get; set; }
    }
}
