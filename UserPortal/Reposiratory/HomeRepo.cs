using MiSAP.Models;
using System.Collections;
using System.Data;

namespace UserPortal.Reposiratory
{
    public class HomeRepo
    {
        Dal_SQL objSql = new Dal_SQL();
        public DataTable GetSalesOrder(Hashtable ht) 
        {
            return objSql.select_proc("GET_SALES_ORDER_INFO", ht);
        }
    }
}
