using Microsoft.AspNetCore.Mvc;

namespace UserPortal.Controllers
{
    public class AccessRightController : Controller
    {
        public IActionResult Index(string id)
        {
            ViewBag.ButtonRightsScreenId = id;
            return View();
        }
    }
}
