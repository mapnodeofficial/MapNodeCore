using Core.Application.ViewModels.System;
using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Application.ViewModels.Report
{
    public class ReportViewModel
    {
        public ReportViewModel()
        {
        }

        public int TotalMember { get; set; }
        public int TodayMember { get; set; }
        public int TotalMemberVerifyEmail { get; set; }
        public int TotalMemberInVerifyEmail { get; set; }

        public decimal TotalBNBDeposit { get; set; }
        public decimal TotalBNBWithdraw { get; set; }
        public decimal TodayBNBDeposit { get; set; }
        public decimal TodayBNBWithdraw { get; set; }

        public decimal TotalFBDeposit { get; set; }
        public decimal TotalFBWithdraw { get; set; }
        public decimal TodayFBDeposit { get; set; }
        public decimal TodayFBWithdraw { get; set; }

        public decimal TotalFSDeposit { get; set; }
        public decimal TotalFSWithdraw { get; set; }
        public decimal TodayFSDeposit { get; set; }
        public decimal TodayFSWithdraw { get; set; }
    }
}
