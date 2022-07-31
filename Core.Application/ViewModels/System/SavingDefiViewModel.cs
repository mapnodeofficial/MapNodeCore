using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.System
{
    public class SavingDefiViewModel
    {
        public string AddressTo { get; set; }
        public string AddressFrom { get; set; }
        public SavingDefiTransactionState TransactionState { get; set; }
        public string TransactionStateName { get; set; }
        public decimal USDAmount { get; set; }
        public decimal TokenAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string TokenTransactionHash { get; set; }
        public int Timeline { get; set; }
        public int TokenConfigId { get; set; }
        public string TokenConfigName { get; set; }
        public string TokenConfigCode { get; set; }
        public string TokenConfigImage { get; set; }
        public string Remarks { get; set; }
    }
}
