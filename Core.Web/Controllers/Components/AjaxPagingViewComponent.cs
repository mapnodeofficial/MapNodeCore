using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BeCoreApp.Controllers.Components
{
    public class AjaxPagingViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
