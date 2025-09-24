using MiSAP.Models;
using System.Collections;
using System.Data;


namespace UserPortal.Reposiratory
{
    public class MenuRepo
    {
         Dal_SQL objdata=new Dal_SQL();
        public DataTable GetMenu(Hashtable ht)
        {
            return objdata.select_proc("MENU_RTR", ht);

        }


    }
}
