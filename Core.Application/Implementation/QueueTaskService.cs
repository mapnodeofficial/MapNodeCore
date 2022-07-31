using Core.Application.Interfaces;
using Core.Application.ViewModels.QueueTask;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class QueueTaskService : IQueueTaskService
    {
        private readonly ILogger<QueueTaskService> _logger;
        private readonly IQueueTaskRepository _queueTaskRepository;
        private readonly ISavingDefiService _savingDefiService;
        private readonly ISaleDefiService _saleDefiService;
        private readonly IUnitOfWork _unitOfWork;
        public QueueTaskService(IQueueTaskRepository queueTaskRepository,
            ILogger<QueueTaskService> logger,
            ISavingDefiService savingDefiService,
            IUnitOfWork unitOfWork,
            ISaleDefiService saleDefiService)
        {
            _logger = logger;
            _queueTaskRepository = queueTaskRepository;
            _savingDefiService = savingDefiService;
            _unitOfWork = unitOfWork;
            _saleDefiService = saleDefiService;
        }

        public List<QueueTask> GetAllQueue(QueueStatus status, string job, int pageSize = 10, bool isOrderDesc = true)
        {
            var query = _queueTaskRepository.FindAll();

            if (status > 0)
                query = query.Where(x => x.Status == status);

            if (!string.IsNullOrEmpty(job))
                query = query.Where(x => x.Job == job);

            if (isOrderDesc)
                query = query.OrderByDescending(x => x.CreatedAt);

            return query.Take(pageSize).ToList();
        }

        public async Task<GenericResult> ProcessVerificationTransaction()
        {
            try
            {
                var hasQueue = await _queueTaskRepository
                    .FindAll(x => x.Job == "VerifyTransaction")
                    .AnyAsync(x => x.Status == QueueStatus.Pending);

                if (hasQueue)
                {
                    _logger.LogInformation("Start to process VerifyTransaction");

                    var queues = await _queueTaskRepository
                        .FindAll(x => x.Job == "VerifyTransaction")
                        .Where(x => x.Status == QueueStatus.Pending)
                        .OrderByDescending(x => x.CreatedAt)
                        .Take(10)
                        .ToListAsync();

                    foreach (var queue in queues)
                    {
                        await TryToProcessQueue(queue);
                    }

                    _logger.LogInformation("End to process VerifyTransaction");
                }

                var hasRetryQueue = await _queueTaskRepository
                    .FindAll(x => x.Job == "VerifyTransaction")
                    .AnyAsync(x => x.Status == QueueStatus.Failed && x.Retry < 20);

                if (hasRetryQueue)
                {
                    _logger.LogInformation("retry to process VerifyTransaction");

                    var queues = await _queueTaskRepository
                        .FindAll(x => x.Job == "VerifyTransaction")
                        .Where(x => x.Status == QueueStatus.Failed && x.Retry < 20)
                        .OrderByDescending(x => x.CreatedAt)
                        .Take(10)
                        .ToListAsync();

                    foreach (var retryQueue in queues)
                    {
                        await TryToProcessQueue(retryQueue, true);
                    }

                    _logger.LogInformation("End to process VerifyTransaction");
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Internal System {@0}", e);
            }

            return new GenericResult();
        }

        private async Task TryToProcessQueue(QueueTask queue, bool isRetry = false)
        {
            try
            {
                _logger.LogInformation("Execute {@0}", queue);

                var result = await ProcessQueue(queue, isRetry);

                if (result.Success)
                {
                    _queueTaskRepository.Remove(queue);
                    _unitOfWork.Commit();

                    _logger.LogInformation("Execute Successed");
                }
                else
                {
                    _logger.LogInformation($"Execute Failed, Retry: {queue.Retry}");
                    queue.Status = QueueStatus.Failed;
                    queue.Retry++;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Execute Failed");
                _logger.LogError("Execute Failed: {@0}", e);
                queue.Status = QueueStatus.Failed;
                queue.Retry++;
                _unitOfWork.Commit();
            }
        }

        private async Task<GenericResult> ProcessQueue(QueueTask queue, bool isRetry)
        {
            var model = JsonSerializer.Deserialize<TransactionVerificationSetting>(queue.Setting);

            _logger.LogInformation("Params: {@0}", model);

            if (model.IsSaving)
            {
                if (model.IsBNB)
                {
                    var resultBNB = await _savingDefiService
                        .ProcessVerificationBNBTransaction(
                        model.TransactionHash, model.TempDAppTransactionId, isRetry);

                    return resultBNB;
                }

                GenericResult resultSmart = await _savingDefiService
                    .ProcessVerificationSmartContractTransaction(
                    model.TransactionHash, model.TempDAppTransactionId, isRetry);

                return resultSmart;
            }
            else
            {
                var resultBNB = await _saleDefiService
                    .ProcessVerificationBNBTransaction(
                    model.TransactionHash, model.TempDAppTransactionId, isRetry);

                return resultBNB;
            }
        }
    }
}
