using System.Collections;
using System.Data;

namespace UserPortal.Service
{
    public interface IHomeService
    {
        public DataTable GetSalesOrderInfo(Hashtable ht);
    }
}
