using MiSAP.Models;
using System.Collections;
using System.Data;

namespace UserPortal.Reposiratory
{
    public class UserRegistrationRepo
    {
        Dal_SQL objdal = new Dal_SQL();

        public string SaveUser(Hashtable ht)
        {
            return objdal.StringExecuteNonQuery("SAVE_USER_MASTER", ht, "Portal");
        }
        public DataTable GetUser(Hashtable ht)
        {
            return objdal.select_proc("GET_USER_MASTER", ht);
        }
    }
}
