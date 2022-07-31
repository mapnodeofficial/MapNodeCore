using System;

namespace BeCoreApp.Application.ViewModels.Report
{
    public class GameInfoCrashViewModel
    {
        public GameInfoCrashViewModel()
        {
        }
        public long Id { get; set; }
        public long GameCrash { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsEnded { get; set; }
        public long HashId { get; set; }
        public string Status { get; set; }
    }
}