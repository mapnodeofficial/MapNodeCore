using System;

namespace BeCoreApp.Application.ViewModels.Report
{
    public class GameReportbalanceViewModel
    {
        public GameReportbalanceViewModel()
        {
        }
        public Nullable<int> RowNumber { get; set; }
        public Nullable<int> ReasonId { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> ValueCoin { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> ReasonType { get; set; }
        public Nullable<decimal> CofferGold { get; set; }
        public Nullable<decimal> WalletGold { get; set; }
    }
}