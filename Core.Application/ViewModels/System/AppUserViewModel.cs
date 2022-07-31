using BeCoreApp.Data.Enums;
using Core.Application.ViewModels.Blog;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.Valuesshare;
using Core.Data.Enums;
using System;
using System.Collections.Generic;

namespace Core.Application.ViewModels.System
{
    public class AppUserViewModel
    {
        public AppUserViewModel()
        {
            Roles = new List<string>();
            Supports = new List<SupportViewModel>();
            SaleBlocks = new List<SaleBlockViewModel>();
            WalletTransactions = new List<WalletTransactionViewModel>();
            TicketTransactions = new List<TicketTransactionViewModel>();
        }

        public Guid? Id { set; get; }
        public Guid? ReferralId { get; set; }
        public string Sponsor { get; set; }
        public bool IsSystem { get; set; }
        public string Email { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
        public bool EmailConfirmed { get; set; }
        public bool Enabled2FA { get; set; }
        public string AuthenticatorCode { get; set; }
        public string ReferalLink { get; set; }


        public Status Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string ByCreated { get; set; }
        public string ByModified { get; set; }
        public bool HasClaimed { get; set; }

        public string BNBBEP20PublishKey { get;set;}
        public string PublishKey { get;set;}
        public string PrivateKey { get;set;}
        public List<string> Roles { get; set; }

        public string BEP20PublishKey { get;set;}
        public decimal SavingAmount { get; set; }
        public decimal SavingAffiliateAmount { get; set; }
        public SavingLevel SavingLevel { get; set; }
        public string RoleName { get;set;}

        public bool IsLeader { get;set;}

        public bool IsLockedOut { get;set;}

        public List<SupportViewModel> Supports { set; get; }
        public List<TicketTransactionViewModel> TicketTransactions { set; get; }
        public List<WalletTransactionViewModel> WalletTransactions { set; get; }
        public List<SaleBlockViewModel> SaleBlocks { set; get; }

    }
}
