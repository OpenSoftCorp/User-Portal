using Microsoft.AspNetCore.Mvc;
using System.Collections;
using UserPortal.Models.Entities;
using UserPortal.Service;

namespace UserPortal.Controllers
{
    public class UserRegistrationController : Controller
    {
        Hashtable objht = new Hashtable();

        private readonly IUserRegistrationService _objRegService;
        public UserRegistrationController(IUserRegistrationService regService) 
        {
            _objRegService = regService;
        }
        public IActionResult Index(string id)
        {
            ViewBag.ButtonRightsScreenId = id;
            return View();
        }


        public JsonResult SaveUser(string UserCode, string UserName, string MobileNo, string Email, string Designation, string Status, string UserType)
        {

            try
            {

                objht.Clear();
                objht.Add("CreatedBy", HttpContext.Session.GetString("Code").ToString());
                objht.Add("UserCode", UserCode == null ? "" : UserCode);
                objht.Add("UserName", UserName == null ? "" : UserName);
                objht.Add("UserType", UserType == null ? "" : UserType);
                objht.Add("MobileNo", MobileNo);
                objht.Add("Email", Email == null ? "" : Email);
                objht.Add("Designation", Designation);
                objht.Add("Status", Status == "true" ? "1" : "0");
                string Result = _objRegService.SaveUser(objht);
                if (Result != "-1")
                {
                    string[] Results = Result.Split("-");
                    if (Results[0].ToString() == "A")
                    {
                        string Message = "User Registration Successfully";
                       
                        //SendMail(Message, Email, Results[1].ToString());
                    }
                    if (Results[0].ToString() == "M")
                    {
                        string Message = "User Modiefied Successfully";

                        //SendMail(Message, Email, Results[1].ToString());
                    }
                    return GetUser(Results[1].ToString());
                }
                else
                {
                    return new JsonResult("some error occured") { Value = "some error occured", StatusCode = 500 };

                }

            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message) { Value = ex.Message, StatusCode = 500 };

            }

        }

        public JsonResult GetUser(string UserCode)
        {
            try
            {
                objht.Clear();
                objht.Add("Code", UserCode == null ? "" : UserCode);
                List<UserRegistrationInfo> objUser = _objRegService.GetUser(objht);
                if (objUser.Count > 0)
                {
                    JsonResult data = Json(objUser);
                    return data;
                }
                else
                {
                    return new JsonResult("No data found") { Value = "No data found", StatusCode = 500 };

                }
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message) { Value = ex.Message, StatusCode = 500 };

            }
        }
    }
}
