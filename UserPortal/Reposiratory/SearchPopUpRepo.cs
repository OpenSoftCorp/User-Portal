using CustomerVendorPortal.Models.Data_Utility;
using MiSAP.Models;
using System.Collections;
using System.Data;

namespace UserPortal.Reposiratory
{
    public class SearchPopUpRepo
    {
        DAL_HANA objdalHANA = new DAL_HANA();
        Dal_SQL objdalSQL = new Dal_SQL();
        public Hashtable objHT = new Hashtable();

        DataTable dt;

        #region HANA Search
        public DataTable ShowRegUserCode()
        {
            return objdalHANA.select_procSAP("USER_SEARCH", objHT);

        }

        #endregion


        #region SQL Search
        public DataTable ShowRGUserCode()
        {
            return objdalSQL.select_procSAP("USERCODE_CCH", objHT);

        }
        #endregion

    }
}
