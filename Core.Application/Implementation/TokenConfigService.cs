using Core.Application.Interfaces;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.Saving;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Implementation
{
    public class TokenConfigService : ITokenConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenConfigRepository _tokenConfigRepository;

        public TokenConfigService
            (
            IUnitOfWork unitOfWork,
            ITokenConfigRepository tokenConfigRepository
            )
        {
            _unitOfWork = unitOfWork;
            _tokenConfigRepository = tokenConfigRepository;
        }

        public PagedResult<TokenConfigViewModel> GetAllPaging(string keyword = "",
            TokenConfigType type = 0, int pageIndex = 1, int pageSize = 20)
        {
            var timeLines = ((SavingTimeLine[])Enum
                .GetValues(typeof(SavingTimeLine)))
                .Select(c => new EnumModel()
                {
                    Value = (int)c,
                    Name = c.GetDescription()
                }).ToList();

            var query = _tokenConfigRepository.FindAll();

            query = query.Where(x => !x.IsDeleted);

            if (type > 0)
                query = query.Where(x => x.Type == type);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Name.Contains(keyword)
                || x.TokenCode.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderBy(x => x.ArrangeOrder)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable()
                .Select(x => PrepareTokenConfigSavingPeriods(x, timeLines)).ToList();

            return new PagedResult<TokenConfigViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        static TokenConfigViewModel PrepareTokenConfigSavingPeriods(
            TokenConfig config, List<EnumModel> timeLines = null)
        {
            var model = new TokenConfigViewModel()
            {
                Id = config.Id,
                ContractAddress = config.ContractAddress,
                MaxSaving = config.MaxSaving,
                Decimals = config.Decimals,
                MinSaving = config.MinSaving,
                Name = config.Name,
                TokenCode = config.TokenCode,
                TokenImageUrl = config.TokenImageUrl,
                TotalSaving = config.TotalSaving,
                TotalSupply = config.TotalSupply,
                TypeName = config.Type.GetDescription(),
                Type = config.Type,
                CreatedOn = config.CreatedOn,
                PerTotalSaving = (int)((config.TotalSaving / config.TotalSupply) * 100),
                Interest180Day = config.Interest180Day,
                Interest270Day = config.Interest270Day,
                Interest360Day = config.Interest360Day,
                Interest720Day = config.Interest720Day,
                TimeLines = timeLines,
                AbiConfig = config.AbiConfig,
                ArrangeOrder = config.ArrangeOrder,
                FeeWithdraw = config.FeeWithdraw,
                MaxDeposit = config.MaxDeposit,
                MaxWithdraw = config.MaxWithdraw,
                MinDeposit = config.MinDeposit,
                MinWithdraw = config.MinWithdraw,
                MinSwap = config.MinSwap,
                MinTransfer = config.MinTransfer,
                SwapFee = config.SwapFee,
                TransferFee = config.TransferFee
                
            };

            List<decimal> rates = new()
            {
                config.Interest180Day,
                config.Interest270Day,
                config.Interest360Day,
                config.Interest720Day
            };

            model.MaxInterestRate = rates.Max();

            return model;
        }

        public TokenConfigViewModel GetById(int id)
        {
            var model = _tokenConfigRepository.FindById(id);

            if (model == null)
                return null;

            return PrepareTokenConfigSavingPeriods(model);

        }

        public TokenConfigViewModel GetByCode(string tokenCode)
        {
            var model = _tokenConfigRepository.FindAll(x => x.TokenCode.Equals(tokenCode)).SingleOrDefault();

            if (model == null)
                return null;

            return PrepareTokenConfigSavingPeriods(model);

        }

        public void Delete(int id)
        {
            var token = _tokenConfigRepository.FindById(id);

            token.IsDeleted = true;

            token.ModifiedOn = DateTime.Now;
            _tokenConfigRepository.Update(token);

            Save();
        }

        public void Update(TokenConfigViewModel model)
        {
            var token = _tokenConfigRepository.FindById(model.Id);

            token.ContractAddress = model.ContractAddress;
            token.TotalSaving = model.TotalSaving;
            token.MinSaving = model.MinSaving;
            token.Decimals = model.Decimals;
            token.MaxSaving = model.MaxSaving;
            token.TotalSupply = model.TotalSupply;
            token.TokenCode = model.TokenCode;
            token.TokenImageUrl = model.TokenImageUrl;
            token.Type = model.Type;
            token.ModifiedOn = DateTime.Now;
            token.Interest180Day = model.Interest180Day;
            token.Interest270Day = model.Interest270Day;
            token.Interest360Day = model.Interest360Day;
            token.Interest720Day = model.Interest720Day;
            token.MinDeposit = model.MinDeposit;
            token.MaxDeposit = model.MaxDeposit;
            token.MinWithdraw = model.MinWithdraw;
            token.MaxWithdraw = model.MaxWithdraw;
            token.FeeWithdraw = model.FeeWithdraw;


            _tokenConfigRepository.Update(token);

            Save();
        }

        public void Add(TokenConfigViewModel model)
        {
            var token = new TokenConfig()
            {
                ContractAddress = model.ContractAddress,
                Type = model.Type,
                TotalSupply = model.TotalSupply,
                TokenImageUrl = model.TokenImageUrl,
                TokenCode = model.TokenCode,
                TotalSaving = model.TotalSaving,
                MaxSaving = model.MaxSaving,
                Decimals = model.Decimals,
                MinSaving = model.MinSaving,
                Name = model.Name,
                CreatedOn = DateTime.Now,
                IsDeleted = false,
                MaxDeposit = model.MaxDeposit,
                MinDeposit = model.MinDeposit,
                MaxWithdraw = model.MaxWithdraw,
                MinWithdraw = model.MinWithdraw,
                Interest180Day = model.Interest180Day,
                Interest270Day = model.Interest270Day,
                Interest360Day = model.Interest360Day,
                Interest720Day = model.Interest720Day,
            };

            _tokenConfigRepository.Add(token);
            Save();
        }


        public decimal GetInterestRate(int id, SavingTimeLine timeline)
        {
            var model = _tokenConfigRepository.FindById(id);
            if (model == null)
                return 0;

            if (timeline == SavingTimeLine.Day180)
            {
                return model.Interest180Day;
            }
            else if (timeline == SavingTimeLine.Day270)
            {
                return model.Interest270Day;
            }
            else if (timeline == SavingTimeLine.Day360)
            {
                return model.Interest360Day;
            }
            else if (timeline == SavingTimeLine.Day720)
            {
                return model.Interest720Day;
            }
            else
            {
                return 0;
            }
        }

        public decimal GetDailyInterestRate(int id, SavingTimeLine timeline)
        {
            var model = _tokenConfigRepository.FindById(id);

            if (model == null)
                return 0;

            if (timeline == SavingTimeLine.Day180)
            {
                return (decimal)model.Interest180Day / (int)SavingTimeLine.Day180;
            }
            else if (timeline == SavingTimeLine.Day270)
            {
                return (decimal)model.Interest270Day / (int)SavingTimeLine.Day270;
            }
            else if (timeline == SavingTimeLine.Day360)
            {
                return (decimal)model.Interest360Day / (int)SavingTimeLine.Day360;
            }
            else if (timeline == SavingTimeLine.Day720)
            {
                return (decimal)model.Interest360Day / (int)SavingTimeLine.Day360;
            }
            else
            {
                return 0;
            }
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
