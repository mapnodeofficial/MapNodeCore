using Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Extensions;
using System.Security.Claims;
using Core.Utilities.Constants;
using Core.Application.ViewModels.System;

namespace Core.Areas.Admin.Components
{
    public class SideBarViewComponent : ViewComponent
    {
        private IMenuGroupService _menuGroupService;
        private IMenuItemService _menuItemService;

        public SideBarViewComponent(
            IMenuGroupService menuGroupService,
            IMenuItemService menuItemService)
        {
            _menuGroupService = menuGroupService;
            _menuItemService = menuItemService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var roleId = ((ClaimsPrincipal)User).GetSpecificClaim("RoleId");
            var menuGroup = _menuGroupService.GetByRoleId(roleId);
            if (menuGroup == null)
                return View(new List<string>());

            var userName = ((ClaimsPrincipal)User).GetSpecificClaim("UserName");

            var menuContent = _menuItemService.GetMenuString(menuGroup.Id, userName);

            return View(new List<string> { new string(menuContent) });
        }
    }
}
