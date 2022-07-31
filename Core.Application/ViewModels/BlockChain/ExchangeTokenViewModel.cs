using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.BlockChain
{
    public class ExchangeTokenViewModel
    {
        public decimal OrderBNB { get; set; }
        public decimal AmountToken { get; set; }

        public decimal TokenPrice { get; set; }
        public string TokenPriceString { get; set; }
        public decimal BNBPrice { get; set; }
        public SaleDefiTransactionType Type { get; set; }

        public string BEP20Address { get; set; }

        public decimal BNBBalance { get; set; }
    }
}
