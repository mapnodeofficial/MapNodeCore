using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("QueueTasks")]
    public class QueueTask : DomainEntity<int>
    {
        public string Job { get; set; }
        public string Setting { get; set; }
        public string Description { get; set; }
        public QueueStatus Status { get; set; }
        public int Retry { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
