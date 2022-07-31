using Core.Data.Enums;
using System;

namespace Core.Application.ViewModels.Saving
{
    public class ProcessSavingModel
    {
        public Guid AppUserId { get;set;}

        public decimal Amount { get;set;}

        public int TokenConfigId { get;set;}

        public SavingTimeLine Timeline { get;set;}

        public string AddressFrom { get;set;}

        public Guid DaapTransactionId { get;set;}

        public decimal AmountUSD { get;set;}

        public decimal InterestRate { get;set;}

        public decimal ExpectedInterest { get;set;}


    }
}
