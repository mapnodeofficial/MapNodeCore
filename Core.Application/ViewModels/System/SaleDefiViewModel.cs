using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.System
{
    public class SaleDefiViewModel
    {
        public string AddressTo { get; set; }
        public string AddressFrom { get; set; }
        public Guid AppUserId { get; set; }
        public SaleDefiTransactionState TransactionState { get; set; }
        public string TransactionStateName { get; set; }
        public SaleDefiTransactionType Type { get; set; }
        public string TypeName { get; set; }
        public decimal USDAmount { get; set; }
        public decimal TokenAmount { get; set; }
        public decimal BNBAmount { get; set; }
        public string BNBTransactionHash { get; set; }
        public string FBTransactionHash { get; set; }
        public bool IsDevice { get; set; }
        public string WalletType { get; set; }
        public string Remarks { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

}
