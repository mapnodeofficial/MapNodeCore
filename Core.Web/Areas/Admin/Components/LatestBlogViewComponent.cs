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
    public class LatestBlogViewComponent : ViewComponent
    {
        private IBlogService _blogService;

        public LatestBlogViewComponent(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var blogs = _blogService.GetLatestBlogs(4);
            return View(blogs);
        }
    }
}
