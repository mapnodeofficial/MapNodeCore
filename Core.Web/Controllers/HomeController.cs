using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Models;
using Core.Application.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Core.Data.Enums;
using Microsoft.Extensions.Logging;

namespace Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBlogService _blogService;
        private readonly ILogger<HomeController> _logger;
        public HomeController(
            IBlogService blogService,
            IConfiguration configuration,
            ILogger<HomeController> logger
            )
        {
            _logger = logger;
            _blogService = blogService;
            _configuration = configuration;
        }

        //[ResponseCache(CacheProfileName ="Default")]
        public IActionResult Index(string id= "MN_YMRUO1K0UH")
        {
            ViewBag.RefCode = id;
            HomeViewModel model = new();
            model.HomeBlogs = _blogService.GetHomeBlogs();
            return View(model);
        }

        //public IActionResult Presale()
        //{
        //    return View();
        //}
        //[Route("Reflink/")]
        //public IActionResult Reflink()
        //{

        //    _logger

        //    ViewBag.RefCode = "101000";



        //    return View("Index");
        //}

        //[Route("Reflink/{id}")]
        //public IActionResult Reflink(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        ViewBag.RefCode = "101000";
        //        return View("Index");
        //    }

        //    ViewBag.RefCode = id;
        //    return View("Index");
        //}

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        
    }
}
