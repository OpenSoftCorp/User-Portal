using System.Collections;
using System.Data;
using UserPortal.Reposiratory;

namespace UserPortal.Service
{
    public class HomeService : IHomeService
    {
        private readonly HomeRepo objRepo;

        public HomeService(HomeRepo homeRepo) 
        {
            objRepo = homeRepo;
        }
        public DataTable GetSalesOrderInfo(Hashtable ht)
        {
            return objRepo.GetSalesOrder(ht);
        }
    }
}
