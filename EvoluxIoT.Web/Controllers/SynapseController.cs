using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvoluxIoT.Web.Controllers
{
    public class SynapseController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
