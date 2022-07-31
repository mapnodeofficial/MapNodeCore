using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.Game
{
    public class BuyTicketViewModel
    {
        public int Type { get; set; }

        public int TicketOrder { get; set; }

        public decimal AmountPayment { get; set; }
    }
}
