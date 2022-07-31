using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Data.Enums;
using Core.Models.BlogViewModels;
using Core.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace Core.Controllers
{
    public class BlogController : Controller
    {
        private IMenuGroupService _menuGroupService;
        private IMenuItemService _menuItemService;
        private IBlogCategoryService _blogCategoryService;
        private IBlogService _blogService;
        private IConfiguration _configuration;

        public BlogController(
            IMenuGroupService menuGroupService,
            IMenuItemService menuItemService,
            IBlogCategoryService blogCategoryService,
            IBlogService blogService,
            IConfiguration configuration)
        {
            _menuGroupService = menuGroupService;
            _menuItemService = menuItemService;
            _blogCategoryService = blogCategoryService;
            _blogService = blogService;
            _configuration = configuration;
        }

        [Route("news.html")]
        public IActionResult BlogCategory(int? pageSize, int page = 1)
        {
            var catalog = new CatalogViewModel();
            if (pageSize == null)
                pageSize = _configuration.GetValue<int>("BlogPageSize");

            catalog.PageSize = pageSize;
            catalog.Data = _blogService.GetAllPaging("", "", "", 0, page, pageSize.Value);

            ViewBag.Title = "News";

            return View(catalog);
        }

        [Route("{alias}-b.{id}.html", Name = "BlogDetail")]
        public IActionResult BlogDetail(int id)
        {
            var catalog = new DetailViewModel();
            catalog.Blog = _blogService.GetById(id);
            catalog.BlogCategory = _blogCategoryService.GetById(catalog.Blog.BlogCategoryId);
            catalog.BlogTags = _blogService.GetListTagByBlogId(catalog.Blog.Id);
            return View(catalog);
        }
    }
}
