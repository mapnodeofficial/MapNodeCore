using Core.Data.Entities;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Core.Web.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public virtual async Task<bool> VerifyCode(
            string authenticatorCode, UserManager<AppUser> userManager, AppUser appUser = null)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            appUser = appUser ?? await userManager.FindByIdAsync(userId);

            if (!appUser.TwoFactorEnabled) return true;

            return await userManager.VerifyTwoFactorTokenAsync(
                appUser, TokenOptions.DefaultAuthenticatorProvider, authenticatorCode);
        }

        public string CurrentUserId
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return User.GetSpecificClaim("UserId");
                }
                return null;

            }
        }

        public string UserName
        {

            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userNameClaim = User.GetSpecificClaim("UserName");
                    return userNameClaim;
                }

                return null;
            }

        }
    }
}
