using System.Collections;
using System.Data;
using UserPortal.Models.Entities;
using UserPortal.Reposiratory;

namespace UserPortal.Service
{
    public class MenuService : IMenuService
    {
        
        private  MenuRepo objmenuRepo;
      
        public List<MenuesInfo> objManues = new List<MenuesInfo>();
    
        public MenuService(MenuRepo menuRepo) 
        {
            objmenuRepo = menuRepo;
        }
        public List<MenuesInfo> GetMenues(Hashtable objHT)
        {
            DataTable dt = objmenuRepo.GetMenu(objHT);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                objManues.Add(new MenuesInfo()
                {
                    ScreenId = dt.Rows[i]["ScreenId"].ToString(),
                    ParentId = dt.Rows[i]["ParentId"].ToString(),
                    ActionName = dt.Rows[i]["ActionName"].ToString(),
                    ControllerName = dt.Rows[i]["ControllerName"].ToString(),
                    Icon = dt.Rows[i]["Icon"].ToString(),
                    ButtonRights = dt.Rows[i]["ButtonRights"].ToString(),
                    Flag = dt.Rows[i]["Flag"].ToString(),
                    Target = dt.Rows[i]["Target"].ToString(),
                    hasArrow = dt.Rows[i]["hasArrow"].ToString(),
                    SCREENNAME = dt.Rows[i]["SCREENNAME"].ToString(),



                });
            }

            return objManues;
        }
    }
}
