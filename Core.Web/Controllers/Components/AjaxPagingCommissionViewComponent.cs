using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BeCoreApp.Controllers.Components
{
    public class AjaxPagingCommissionViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
