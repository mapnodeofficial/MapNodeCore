using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.QueueTask
{
    public class TransactionVerificationSetting
    {
        public string TempDAppTransactionId { get; set; }
        public string TransactionHash { get; set; }

        public bool IsBNB { get; set; }

        public bool IsSaving { get; set; }
    }
}
