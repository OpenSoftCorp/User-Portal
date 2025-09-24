using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;
using UserPortal.Models.Entities;
using UserPortal.Service;

namespace UserPortal.Controllers
{
    public class LoginController : Controller
    {
        Hashtable ht=new Hashtable();
        private readonly ILoginService ObjLogin;

        public LoginController(ILoginService objLogin)
        {
            ObjLogin = objLogin;
        }

        public IActionResult Index()
        {  
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginInfo objLogin)
        {

            try
            {
                if (objLogin == null)
                {
                    ViewBag.Error = "some error occured";
                }
                else
                {
                    DataTable dt = ObjLogin.ValidateUser(objLogin.UserName, objLogin.Password);
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0]["ErrorMsg"].ToString() == "")
                        {
                            HttpContext.Session.SetString("Code", dt.Rows[0]["Code"].ToString());
                            HttpContext.Session.SetString("Name", dt.Rows[0]["Name"].ToString());
                            HttpContext.Session.SetString("Email", dt.Rows[0]["EMAIL"].ToString());
                            HttpContext.Session.SetString("Role", dt.Rows[0]["Role"].ToString());
                            HttpContext.Session.SetString("MOBILENO", dt.Rows[0]["MOBILENO"].ToString());
                            //LogIn Histort Start
                            ObjLogin.LogInHistory(dt.Rows[0]["Code"].ToString(), dt.Rows[0]["Name"].ToString(), HttpContext.Session.Id);
                            // LogIn History End


                            if (dt.Rows[0]["Role"].ToString().Trim() == "S") //Super Admin
                            {
                                return Redirect("~/Home/Index");
                            }
                            else if (dt.Rows[0]["Role"].ToString().Trim() == "BU") //Branch Admin
                            {
                                return Redirect("~/Home/Index");
                            }
                        }
                        else 
                        {
                            ViewBag.Message = dt.Rows[0]["ErrorMsg"].ToString();
                        }
                      
                    }
                    else
                    {
                        ViewBag.Message = "verification failed";
                    }
                }
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
            }

            return View(objLogin);
        }



       
    }
}
