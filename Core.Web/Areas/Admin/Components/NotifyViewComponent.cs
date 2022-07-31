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
    public class NotifyViewComponent : ViewComponent
    {
        private INotifyService _notifyService;

        public NotifyViewComponent(INotifyService notifyService)
        {
            _notifyService = notifyService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notifys = _notifyService.GetLast(3);
            return View(notifys);
        }
    }
}
