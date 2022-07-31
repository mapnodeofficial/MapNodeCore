using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.System;
using Core.Application.ViewModels.Valuesshare;
using Core.Data.Entities;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Core.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class UserService : IUserService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDrinkAccessCodeRepository _drinkAccessCodeRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UserService(UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IAuthenticationService authenticationService,
            IBlockChainService blockChainService,
            IDrinkAccessCodeRepository drinkAccessCodeRepository,
            IUnitOfWork unitOfWork)
        {
            _drinkAccessCodeRepository = drinkAccessCodeRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
        }

        #region Customer

        public StatementUserViewModel GetStatementUser(string keyword, int type)
        {
            var model = new StatementUserViewModel();

            var query = _userManager.Users
                .Where(x => x.IsSystem == false && x.EmailConfirmed == true
            );

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Email.Contains(keyword)
                || x.Sponsor.Contains(keyword));

            query = query.Where(x => CommonConstants.MemberAccessDenied.Where(ma => ma.Email.ToLower() == x.Email.ToLower()).Count() == 0);

            model.AppUsers = query.Select(x => new AppUserViewModel()
            {
                Id = x.Id,
                Sponsor = $"{x.Sponsor}",
                Email = x.Email,
            }).ToList();

            model.TotalMember = model.AppUsers.Count();

            return model;
        }

        public PagedResult<AppUserViewModel> GetAllCustomerPagingAsync(string keyword, int page, int pageSize)
        {
            var query = _userManager.Users.Where(x => x.IsSystem == false);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Email.Contains(keyword)
                || x.UserName.Contains(keyword)
                || x.Sponsor.Contains(keyword));

            int totalRow = query.Count();



            var data = query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new AppUserViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    EmailConfirmed = x.EmailConfirmed,
                    Sponsor = $"{x.Sponsor}",
                    Email = x.Email,
                    Status = x.Status,
                    DateCreated = x.DateCreated,
                    BNBBEP20PublishKey = x.BNBBEP20PublishKey,
                    BEP20PublishKey = x.PublishKey,
                    IsLockedOut = x.LockoutEnabled && x.LockoutEnd != null
                }).ToList();

            PrepareUserRole(data).GetAwaiter().GetResult();

            var paginationSet = new PagedResult<AppUserViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };

            return paginationSet;
        }

        private async Task PrepareUserRole(List<AppUserViewModel> users)
        {
            foreach (var user in users)
            {
                var currentU = await _userManager.FindByIdAsync(user.Id.ToString());
                var roles = await _userManager.GetRolesAsync(currentU);

                user.RoleName = string.Join(",", roles);
            }


        }

        public async Task<NetworkViewModel> GetNetworkInfo(string id)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer != null)
            {
                var model = new NetworkViewModel
                {
                    Email = customer.Email,
                    Member = customer.UserName,
                    Sponsor = $"{customer.Sponsor}",
                    EmailConfirmed = customer.EmailConfirmed,
                    ReferalLink = $"https://metadefi.network/register?sponsor=MTD{customer.Sponsor}",
                    CreatedDate = customer.DateCreated
                };
                //model.ReferralAddress = customer.ReferralAddress;
                return model;
            }
            else
            {
                var model = new NetworkViewModel
                {
                    ReferalLink = $"https://metadefi.network/register?sponsor={id}",
                    CreatedDate = DateTime.Now
                };
                return model;
            }

        }

        public async Task<NetworkViewModel> GetTotalNetworkInfo(string id)
        {
            var model = new NetworkViewModel();

            var customer = await _userManager.FindByIdAsync(id);

            var userList = _userManager.Users.Where(x => x.IsSystem == false);

            var f1Customers = userList.Where(x => x.ReferralId == customer.Id);
            var f1CustomerCount = f1Customers.Count();
            model.TotalF1 = f1CustomerCount;
            model.TotalMember = f1CustomerCount;

            if (f1CustomerCount > 0)
            {
                var f1CustomerIDs = f1Customers.Select(x => x.Id).ToList();

                var f2Customers = userList.Where(x => f1CustomerIDs.Contains(x.ReferralId.Value));
                var f2CustomerCount = f2Customers.Count();
                model.TotalF2 = f2CustomerCount;
                model.TotalMember += f2CustomerCount;

                if (f2CustomerCount > 0)
                {
                    var f2CustomerIDs = f2Customers.Select(x => x.Id).ToList();
                    var f3Customers = userList.Where(x => f2CustomerIDs.Contains(x.ReferralId.Value));
                    var f3CustomerCount = f3Customers.Count();
                    model.TotalF3 = f3CustomerCount;
                    model.TotalMember += f3CustomerCount;

                    if (f3CustomerCount > 0)
                    {
                        var f3CustomerIDs = f3Customers.Select(x => x.Id).ToList();
                        var f4Customers = userList.Where(x => f3CustomerIDs.Contains(x.ReferralId.Value));
                        var f4CustomerCount = f4Customers.Count();
                        model.TotalF4 = f4CustomerCount;
                        model.TotalMember += f4CustomerCount;

                        if (f4CustomerCount > 0)
                        {
                            var f4CustomerIDs = f4Customers.Select(x => x.Id).ToList();
                            var f5Customers = userList.Where(x => f4CustomerIDs.Contains(x.ReferralId.Value));
                            var f5CustomerCount = f5Customers.Count();
                            model.TotalF5 = f5CustomerCount;
                            model.TotalMember += f5CustomerCount;
                        }
                    }
                }
            }

            return model;
        }



        public PagedResult<AppUserViewModel> GetCustomerReferralPagingAsync(string customerId, int refIndex, string keyword, int page, int pageSize)
        {
            IQueryable<AppUser> dataCustomers = null;
            var userList = _userManager.Users.Where(x => x.IsSystem == false);
            var f1Customers = userList.Where(x => x.ReferralId == new Guid(customerId));
            if (refIndex == 1)
            {
                dataCustomers = f1Customers;
            }
            else
            {
                var f1Ids = f1Customers.Select(x => x.Id).ToList();
                var f2Customers = userList.Where(x => f1Ids.Contains(x.ReferralId.Value));
                if (refIndex == 2)
                {
                    dataCustomers = f2Customers;
                }
                else
                {
                    var f2Ids = f2Customers.Select(x => x.Id).ToList();
                    var f3Customers = userList.Where(x => f2Ids.Contains(x.ReferralId.Value));
                    if (refIndex == 3)
                    {
                        dataCustomers = f3Customers;
                    }
                    else
                    {

                        var f3Ids = f3Customers.Select(x => x.Id).ToList();
                        var f4Customers = userList.Where(x => f3Ids.Contains(x.ReferralId.Value));
                        if (refIndex == 4)
                        {
                            dataCustomers = f4Customers;
                        }
                        else
                        {
                            var f4Ids = f4Customers.Select(x => x.Id).ToList();
                            var f5Customers = userList.Where(x => f4Ids.Contains(x.ReferralId.Value));
                            if (refIndex == 5)
                            {
                                dataCustomers = f5Customers;
                            }
                        }

                    }
                }
            }
            if (!string.IsNullOrEmpty(keyword))
                dataCustomers = dataCustomers.Where(x => x.UserName.Contains(keyword) || x.Email.Contains(keyword));
            int totalRow = dataCustomers.Count();
            var data = dataCustomers.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new AppUserViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Sponsor = $"{x.Sponsor}",
                    EmailConfirmed = x.EmailConfirmed,
                    Email = x.Email,
                    Status = x.Status,
                    ReferralId = x.ReferralId,
                    DateCreated = x.DateCreated,
                }).ToList();

            var userEmails = data.Select(x => x.Email);
            //var claimedUsers = _dappRepository.FindAll(d => userEmails.Contains(d.Email) && d.Type == Data.Enums.DAppTransactionType.Claim && d.DAppTransactionState == Data.Enums.DAppTransactionState.Confirmed).Select(d => d.Email).ToList();

            //foreach (var item in data)
            //{
            //    item.HasClaimed = claimedUsers.Any(u => u == item.Email);
            //}

            return new PagedResult<AppUserViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };
        }

        public AppUserViewModel GetUserByRefCode(string refCode)
        {
            var user = _userManager.Users.SingleOrDefault(x => x.Sponsor == refCode);
            if (user == null)
                return null;


            return new AppUserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Sponsor = user.Sponsor,
                EmailConfirmed = user.EmailConfirmed,
                Email = user.Email,
                Status = user.Status,
                ReferralId = user.ReferralId,
                DateCreated = user.DateCreated,
            };
        }

        #endregion Customer

        #region User System
        public PagedResult<AppUserViewModel> GetAllUser(string keyword)
        {
            //var query = _userManager.Users.Where(x => x.IsSystem);
            var query = _userManager.Users;
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.UserName.Contains(keyword) || x.Email.Contains(keyword));

            int totalRow = query.Count();
            var data = query
                .Select(x => new AppUserViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    Status = x.Status,
                    DateCreated = x.DateCreated
                }).ToList();

            var paginationSet = new PagedResult<AppUserViewModel>()
            {
                Results = data,
                RowCount = totalRow,
            };

            return paginationSet;
        }

        public PagedResult<AppUserViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            var query = _userManager.Users.Where(x => x.IsSystem);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.UserName.Contains(keyword) || x.Email.Contains(keyword));

            int totalRow = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new AppUserViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    Status = x.Status,
                    DateCreated = x.DateCreated
                }).ToList();

            var paginationSet = new PagedResult<AppUserViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };

            return paginationSet;
        }

        public List<AppUserTreeViewModel> GetTreeAll()
        {
            var listData = _userManager.Users.OrderBy(x => x.Sponsor)
                .Select(x => new AppUserTreeViewModel()
                {
                    id = x.Id,
                    text = $"{x.Sponsor}-{x.Email}-{x.EmailConfirmed}-[{x.DateCreated.ToString("MM-dd-yyyy")}]",
                    icon = "fa fa-users text-success",
                    state = new AppUserTreeState { opened = true },
                    data = new AppUserTreeData
                    {
                        referralId = x.ReferralId,
                        rootId = x.Id,
                    }
                });

            if (listData.Count() == 0)
                return new List<AppUserTreeViewModel>();

            var groups = listData.AsEnumerable().GroupBy(i => i.data.referralId);

            var roots = groups.FirstOrDefault(g => g.Key.HasValue == false).ToList();

            if (roots.Count > 0)
            {
                var dict = groups.Where(g => g.Key.HasValue)
                    .ToDictionary(g => g.Key.Value, g => g.ToList());

                for (int i = 0; i < roots.Count; i++)
                    AddChildren(roots[i], dict);
            }
            return roots;
        }

        private void AddChildren(AppUserTreeViewModel root, IDictionary<Guid, List<AppUserTreeViewModel>> source)
        {
            if (source.ContainsKey(root.id))
            {
                root.children = source[root.id].ToList();
                for (int i = 0; i < root.children.Count; i++)
                    AddChildren(root.children[i], source);
            }
            else
            {
                root.icon = "fa fa-user text-danger";
                root.children = new List<AppUserTreeViewModel>();
            }
        }

        public async Task<AppUserViewModel> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var userVm = new AppUserViewModel()
            {
                Id = user.Id,
                EmailConfirmed = user.EmailConfirmed,
                ReferralId = user.ReferralId,
                Enabled2FA = user.TwoFactorEnabled,
                Sponsor = $"{user.Sponsor}",
                IsSystem = user.IsSystem,
                ByCreated = user.ByCreated,
                ByModified = user.ByModified,
                DateModified = user.DateModified,
                UserName = user.UserName,
                Email = user.Email,
                Status = user.Status,
                DateCreated = user.DateCreated,
                Roles = roles.ToList(),
                PublishKey = user.PublishKey,
                PrivateKey = user.PrivateKey,
                BNBBEP20PublishKey = user.BNBBEP20PublishKey,
                SavingAffiliateAmount = user.SavingAffiliateAmount,
                SavingAmount = user.SavingAmount,
                SavingLevel = user.SavingLevel,
                IsLeader = roles.Any(d => d == CommonConstants.LEADER_ROLE)
            };

            if (!user.TwoFactorEnabled)
            {
                userVm.AuthenticatorCode = await _authenticationService.GetAuthenticatorKey(user);
            }

            return userVm;
        }

        public async Task<AppUserViewModel> GetByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var userVm = new AppUserViewModel()
            {
                Id = user.Id,
                EmailConfirmed = user.EmailConfirmed,
                ReferralId = user.ReferralId,
                Enabled2FA = user.TwoFactorEnabled,
                ReferalLink = $"{CommonConstants.TOKEN_CODE}/register?sponsor={CommonConstants.TOKEN_CODE}{user.Sponsor}",
                Sponsor = $"{user.Sponsor}",
                IsSystem = user.IsSystem,
                ByCreated = user.ByCreated,
                ByModified = user.ByModified,
                DateModified = user.DateModified,
                UserName = user.UserName,
                Email = user.Email,
                Status = user.Status,
                DateCreated = user.DateCreated,
                Roles = roles.ToList(),
                BNBBEP20PublishKey = user.BNBBEP20PublishKey,
            };

            return userVm;
        }



        public async Task<bool> AddAsync(AppUserViewModel userVm)
        {
            var user = new AppUser()
            {
                UserName = userVm.UserName,
                Email = userVm.Email,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                IsSystem = true
            };

            var result = await _userManager.CreateAsync(user, userVm.Password);
            if (result.Succeeded && userVm.Roles.Count > 0)
            {
                var appUser = await _userManager.FindByNameAsync(user.UserName);
                if (appUser != null)
                {
                    await _userManager.AddToRolesAsync(appUser, userVm.Roles.ToArray());
                }
            }

            return result.Succeeded;
        }

        public async Task UpdateAsync(AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.ToString());

            //Remove current roles in db
            var currentRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, userVm.Roles.Except(currentRoles).ToArray());
            if (result.Succeeded)
            {
                string[] needRemoveRoles = currentRoles.Except(userVm.Roles).ToArray();
                await _userManager.RemoveFromRolesAsync(user, needRemoveRoles);

                user.Status = userVm.Status;
                user.Email = userVm.Email;
                user.DateModified = DateTime.Now;

                await _userManager.UpdateAsync(user);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
        }

        #endregion User System

        public List<AppUserTreeViewModelAjax> GetMemberTreeAll(string parentId)
        {
            var result = new List<AppUserTreeViewModelAjax>();

            var currentUser = _userManager.FindByIdAsync(parentId).GetAwaiter().GetResult();


            var query = _userManager.Users;

            query = query.Where(x => x.ReferralId == currentUser.Id);

            var childs = query.ToList();

            var currentsIds = new List<Guid>();

            if (childs.Count > 50)
            {
                currentsIds = _userManager.Users
                    .Where(d => d.ReferralId != null)
                    .Select(d => d.ReferralId.Value).ToList();
            }

            foreach (var user in childs)
            {
                bool isHasChild = false;
                if (currentsIds.Count > 0)
                {
                    isHasChild = currentsIds.Any(d => d == user.Id);
                }
                else
                {
                    isHasChild = _userManager.Users
                        .Any(d => d.ReferralId == user.Id);
                }

                result.Add(new AppUserTreeViewModelAjax
                {
                    id = user.Id,
                    text = $"{user.Sponsor}-{user.Email}",
                    icon = $"fa fa-users text-{(isHasChild ? "success" : "danger")}",
                    children = isHasChild
                });
            }
            //}

            return result;
        }

        public string GenerateReferralCode()
        {
            var refCode = TextHelper.GenerateRefCode();

            while (_userManager.Users.Any(d=>d.Sponsor == refCode))
            {
                refCode = TextHelper.GenerateRefCode();
            }

            return refCode;
        }
    }
}
