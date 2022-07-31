using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.System
{
    public class SaleBlockViewModel
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string TransactionHash { get; set; }

        public SaleBlockType Type { get; set; }
        public string TypeName { get; set; }

        public SaleDefiTransactionType RoundType { get; set; }
        public string RoundTypeName { get; set; }

        public Guid AppUserId { get; set; }
        public string AppUserName { get; set; }

        public DateTime StartOn { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }


        public AppUserViewModel AppUser { set; get; }

        public string Sponsor { get;set;}
    }
}
