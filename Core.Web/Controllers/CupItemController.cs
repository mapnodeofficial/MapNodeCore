using Microsoft.AspNetCore.Mvc;

namespace Core.Web.Controllers
{
    public class CupItemController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
