using BeCoreApp.Web.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Core.Web.Controllers
{
    public class RefLinkController : Controller
    {
        private readonly ILogger<RefLinkController> _logger;

        public RefLinkController(ILogger<RefLinkController> logger)
        {
            _logger = logger;
        }

        [Route("Reflink/{id?}")]
        public IActionResult Index(string id)
        {
            _logger.LogInformation($"RefLinkController - {id}");

            if (!string.IsNullOrEmpty(id))
            {
                Remove(ConfigurationConsts.ReferralCookie);
                Set(ConfigurationConsts.ReferralCookie, id, 15);
            }
                

            return RedirectToAction("index", "home", new { id  = id });
        }

        public void Set(string key, string value, int? expireTime)
        {
            _logger.LogInformation($"RefLinkController - Set Cookie {key} - {value}");
            CookieOptions option = new()
            {
                HttpOnly = true,
                IsEssential = true,
            };
            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            Response.Cookies.Append(key, value, option);
        }

        public void Remove(string key)
        {
            Response.Cookies.Delete(key);
        }
    }
}
