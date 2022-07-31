using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Core.Web.Controllers
{
    [Authorize]
    public class ReferralController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<ReferralController> _logger;
        public ReferralController(IUserService userService,
            ILogger<ReferralController> logger)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userModel = await _userService.GetById(CurrentUserId);

            ViewBag.ReferralLink = $"{Request.Scheme}://{Request.Host}/register?sponsor={userModel.Sponsor}";

            return View();
        }

        public async Task<IActionResult> GetMemberTreeNode(string parent)
        {
            try
            {
                if (string.IsNullOrEmpty(parent) || parent.Equals("#"))
                    parent = CurrentUserId;
                else
                {
                    var user = await _userService.GetById(parent);
                    if (user == null)
                        parent = CurrentUserId;
                    else
                        parent = user.Id.ToString();
                }

                var model = _userService.GetMemberTreeAll(parent);
                return new ObjectResult(model);
            }
            catch (System.Exception e)
            {
                _logger.LogError($"GetMemberTreeNode Exception Address {CurrentUserId} - {UserName} - Paremt {parent} {e.Message}");
                _logger.LogError($"GetMemberTreeNode Exception Address {CurrentUserId} - {UserName} Paremt {parent} {e.StackTrace}");
            }

            return new ObjectResult(new AppUserTreeViewModelAjax());
            
        }
    }
}
