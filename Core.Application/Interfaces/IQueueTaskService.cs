using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IQueueTaskService
    {
        List<QueueTask> GetAllQueue(QueueStatus status, string job, int pageSize = 10, bool isOrderDesc = true);

        Task<GenericResult> ProcessVerificationTransaction();

    }
}
