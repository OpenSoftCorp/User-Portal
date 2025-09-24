using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;
using UserPortal.Service;

namespace UserPortal.Controllers
{
    public class HomeController : Controller
    {

        private IHomeService objHomeService;
        Hashtable ht = new Hashtable();
        DataTable dt;
        public HomeController(IHomeService objHomeService)
        {
            this.objHomeService = objHomeService;
        }
      
        public IActionResult Index(string id)
        {
            ViewBag.ButtonRightsScreenId = id;
            try
            {
                ht.Clear();
                ht.Add("UserId", HttpContext.Session.GetString("Code").ToString());
                dt = objHomeService.GetSalesOrderInfo(ht);

            }
            catch(Exception ex) 
            {
                ViewBag.Error = ex.Message;
            }
            return View(dt);
        }
    }
}
