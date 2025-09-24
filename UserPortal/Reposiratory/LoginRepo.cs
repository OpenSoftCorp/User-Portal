using MiSAP.Models;
using System.Collections;
using System.Data;

namespace UserPortal.Reposiratory
{
   
    public class LoginRepo
    {
        Dal_SQL objdal = new Dal_SQL();

        public DataTable ValidateUser(Hashtable ht)
        {
            return objdal.select_proc("VALIDATEUSER", ht);
        }

        public string LogInHistory(Hashtable ht)
        {
            return objdal.StringExecuteNonQuery("SaveLogInHistory", ht,"Portal");
        }
    }
}
