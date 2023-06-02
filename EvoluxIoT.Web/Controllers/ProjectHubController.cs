using Microsoft.AspNetCore.Mvc;

namespace EvoluxIoT.Web.Controllers
{
    public class ProjectHubController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
