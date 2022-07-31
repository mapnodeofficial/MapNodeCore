using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class TicketTransactionService : ITicketTransactionService
    {
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ITicketTransactionRepository _ticketTransactionRepository;
        private readonly IBlockChainService _blockChainService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWalletService _walletService;
        public TicketTransactionService(
            IWalletService walletService,
            IWalletTransactionService walletTransactionService,
            UserManager<AppUser> userManager,
            IBlockChainService blockChainService,
            ITicketTransactionRepository ticketTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
            _blockChainService = blockChainService;
            _userManager = userManager;
            _ticketTransactionRepository = ticketTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        public PagedResult<TicketTransactionViewModel> GetAllPaging(
            string keyword, string username, int pageIndex, int pageSize)
        {
            var query = _ticketTransactionRepository
                .FindAll(x => x.AppUser, tc => tc.TokenConfig);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.Sponsor.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(x => x.AppUser.UserName == username);

            var totalRow = query.Count();
            var data = query.OrderBy(x => x.Status).Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new TicketTransactionViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    Fee = x.Fee,
                    FeeAmount = x.FeeAmount,
                    AmountReceive = x.AmountReceive,
                    Amount = x.Amount,
                    StrAmount = x.Amount.ToString(),
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    Sponsor = $"{ x.AppUser.Sponsor}",
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    Status = x.Status,
                    StatusName = x.Status.GetDescription(),
                    UnitName = x.TokenConfig.TokenCode,
                    DateCreated = x.DateCreated,
                    DateUpdated = x.DateUpdated
                }).ToList();

            return new PagedResult<TicketTransactionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public void Add(TicketTransactionViewModel model)
        {
            var transaction = new TicketTransaction()
            {
                AddressFrom = model.AddressFrom,
                AddressTo = model.AddressTo,
                Fee = model.Fee,
                FeeAmount = model.FeeAmount,
                AmountReceive = model.AmountReceive,
                Amount = model.Amount,
                AppUserId = model.AppUserId,
                Status = model.Status,
                Type = model.Type,
                TokenConfigId = model.TokenConfigId,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            _ticketTransactionRepository.Add(transaction);
        }

        public void Rejected(int id)
        {
            var ticket = _ticketTransactionRepository.FindById(id);

            ticket.Status = TicketTransactionStatus.Rejected;
            ticket.DateUpdated = DateTime.Now;

            _ticketTransactionRepository.Update(ticket);

            _unitOfWork.Commit();

            var wallet = _walletService.GetByTokenIdAndUserId(ticket.TokenConfigId, ticket.AppUserId);

            if (wallet != null)
            {
                wallet.Amount += ticket.Amount;
                _walletService.Update(wallet);

                _walletService.Save();
            }
        }

        public async Task<GenericResult> Approved(int id)
        {
            var ticket = _ticketTransactionRepository.FindById(id, x => x.TokenConfig);

            var tokenConfig = ticket.TokenConfig;

            TransactionReceipt transaction = null;

            decimal balance = ticket.AmountReceive;

            if (tokenConfig.TokenCode.ToUpper() == "BNB")
            {
                var currentBalance = await _blockChainService.
                    GetEtherBalance(CommonConstants.BEP20TransferPuKey, CommonConstants.BEP20Url);

                if (currentBalance <= balance)
                    return new GenericResult(false, "Insufficient account balance");

                transaction = await _blockChainService
                       .SendEthAsync(CommonConstants.BEP20TransferPrKey,
                           ticket.AddressTo, balance, CommonConstants.BEP20Url);

                if (!transaction.Succeeded(true))
                    return new GenericResult(false, "transfer is fail");
            }
            else
            {
                var currentBalance = await _blockChainService.
                    GetERC20Balance(CommonConstants.BEP20TransferPuKey,
                    tokenConfig.ContractAddress, tokenConfig.Decimals, CommonConstants.BEP20Url);

                if (currentBalance < balance)
                    return new GenericResult(false, "Insufficient account balance");

                transaction = await _blockChainService.SendERC20Async(
                           CommonConstants.BEP20TransferPrKey, ticket.AddressTo,
                           tokenConfig.ContractAddress, balance,
                           tokenConfig.Decimals, CommonConstants.BEP20Url);

                if (!transaction.Succeeded(true))
                    return new GenericResult(false, "transfer is fail");
            }

            var walletTransaction = new WalletTransactionViewModel
            {
                AppUserId = ticket.AppUserId,
                AddressFrom = CommonConstants.BEP20TransferPuKey,
                AddressTo = ticket.AddressTo,
                Amount = ticket.Amount,
                FeeAmount = ticket.FeeAmount,
                Fee = ticket.Fee,
                AmountReceive = ticket.AmountReceive,
                TransactionHash = transaction.TransactionHash,
                Type = WalletTransactionType.Withdraw,
                DateCreated = DateTime.Now,
                Unit = tokenConfig.TokenCode
            };

            _walletTransactionService.Add(walletTransaction);
            _walletTransactionService.Save();


            ticket.Status = TicketTransactionStatus.Approved;
            ticket.DateUpdated = DateTime.Now;
            _ticketTransactionRepository.Update(ticket);

            _unitOfWork.Commit();

            return new GenericResult(true, "Approve ticket is success", ticket);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
