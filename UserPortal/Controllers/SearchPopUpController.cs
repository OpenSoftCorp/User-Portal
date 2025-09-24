using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;
using UserPortal.Reposiratory;

namespace UserPortal.Controllers
{
    public class SearchPopUpController : Controller
    {
        Hashtable ht = new Hashtable();
        SearchPopUpRepo objSearch;

        DataTable dt;
       
        public SearchPopUpController(SearchPopUpRepo objrepo) 
        {
          objSearch=  objrepo;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region Search

        public ActionResult ShowSearchPopUpModal(Dictionary<string, string> ParamList, string Type)
        {


            if (Type == "GetUserType")
            {
                ht.Clear();
                foreach (KeyValuePair<string, string> kv in ParamList)
                {
                    ht.Add(kv.Key, kv.Value == null ? "" : kv.Value);
                }
                objSearch.objHT = ht;
                dt = objSearch.ShowRegUserCode();

            }
            return ViewComponent("SearchPopUp", dt);

        }

        public ActionResult ShowSearchPopUpModalSQL(Dictionary<string, string> ParamList, string Type)
        {
            if (Type == "GetUserCode")
            {
                ht.Clear();
                foreach (KeyValuePair<string, string> kv in ParamList)
                {
                    ht.Add(kv.Key, kv.Value == null ? "" : kv.Value);
                }
                objSearch.objHT = ht;
                dt = objSearch.ShowRGUserCode();

            }
         
            return ViewComponent("SearchPopUp", dt);
        }



        #endregion

        #region Modal PopUP List Search

        #endregion

    }
}
