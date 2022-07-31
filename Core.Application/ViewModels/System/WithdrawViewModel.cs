using System;

namespace Core.Application.ViewModels.System
{
    public class WithdrawViewModel
    {
        public int TokenConfigId { get; set; }
        public decimal Amount { get; set; }
        public string Password { get; set; }
    }
}
