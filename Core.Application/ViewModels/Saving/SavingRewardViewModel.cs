using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.Saving
{
    public class SavingRewardViewModel
    {
        public string TokenName { get; set; }
        public string TokenCode { get; set; }
        public string TokenImage { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestedRate { get; set; }

        public Guid? ReferralId { get; set; }
        public string ReferralName { get; set; }

        public SavingRewardType Type { get; set; }
        public string TypeName { get; set; }

        public string Remarks { get; set; }

        public DateTime CreatedOn { get; set; }

        public string AppUserName { get; set; }

        public string Sponsor { get; set; }
    }
}
