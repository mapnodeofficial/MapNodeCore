using BeCoreApp.Application.ViewModels.Report;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Report;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace Core.Application.Implementation
{
    public class ReportService : IReportService
    {
        private readonly ISavingDefiRepository _savingDefiRepository;
        private readonly ITicketTransactionRepository _ticketTransactionRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IBlockChainService _blockChainService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public ReportService(ISavingDefiRepository savingDefiRepository,
                             ITicketTransactionRepository ticketTransactionRepository,
                             IWalletTransactionRepository walletTransactionRepository,
                             IBlockChainService blockChainService,
                             IUnitOfWork unitOfWork,
                             UserManager<AppUser> userManager)
        {
            _savingDefiRepository = savingDefiRepository;
            _ticketTransactionRepository = ticketTransactionRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _blockChainService = blockChainService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public ReportViewModel GetReportInfo(string startDate, string endDate)
        {
            DateTime now = DateTime.Now.Date;

            var appUsers = _userManager.Users;

            var model = new ReportViewModel
            {
                TotalMember = appUsers.Count(),
                TodayMember = appUsers.Count(x => x.DateCreated.Date == now),
                TotalMemberInVerifyEmail = appUsers.Count(x => x.EmailConfirmed == false),
                TotalMemberVerifyEmail = appUsers.Count(x => x.EmailConfirmed == true)
            };

            return model;
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
