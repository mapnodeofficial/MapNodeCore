using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.QueueTask
{
    public class CommissionSetting
    {
        public decimal Amount { get; set; }
        public string AdminAddress { get; set; }
        public Guid UserReferral { get; set; }
        public Guid UserAdmin { get; set; }
        public string Email { get; set; }
        public int Level { get; set; } = 1;
    }
}
