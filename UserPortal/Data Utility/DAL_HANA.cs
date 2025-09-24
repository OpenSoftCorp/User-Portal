using System.Collections;
using System.Data;
using MiSAP.Models;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Data.Odbc;



namespace CustomerVendorPortal.Models.Data_Utility
{
    public class DAL_HANA
    {

        private OdbcConnection Conn;
        private OdbcTransaction odbcTransaction;
        private string objCon;
        Dal_SQL objdal = new Dal_SQL();



        IConfigurationBuilder builder = new ConfigurationBuilder();
        #region SAPConnection
        private OdbcConnection ConnectSAP()
        {
            try
            {
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

                objCon = builder.Build().GetConnectionString("HANA");

                Conn = new OdbcConnection(objCon);

                Conn.Open();
                odbcTransaction = Conn.BeginTransaction();
                return Conn;
            }
            catch (OdbcException ex)
            {
                odbcTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
                //sqlTransaction.Rollback();
                //Conn.Close();
            }
        }
        public DataTable select_procSAP(string procedureName, Hashtable rec)
        {
            try
            {
                DataTable dt = new DataTable();
                Conn = ConnectSAP();
                OdbcDataAdapter sqlDa = new OdbcDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand(procedureName, rec);
                sqlDa.Fill(dt);
                Conn.Close();
                return dt;
            }
            catch (OdbcException ex)
            {
                odbcTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataSet select_procSAPDataSet(string procedureName, Hashtable rec)
        {
            try
            {
                DataSet ds = new DataSet();
                Conn = ConnectSAP();
                OdbcDataAdapter sqlDa = new OdbcDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand(procedureName, rec);
                sqlDa.Fill(ds);
                Conn.Close();
                return ds;
            }
            catch (OdbcException ex)
            {
                odbcTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public OdbcCommand BuildQueryCommand(string storedProcName, Hashtable rec)
        {
            try
            {

                StringBuilder CMDText = new StringBuilder();
                CMDText.Append("call " + storedProcName + " (");
                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    CMDText.Append(myEnumerator.Value.ToString());

                }
                string cmdt = CMDText.ToString() + ")";
               
                OdbcCommand command = new OdbcCommand(cmdt, Conn, odbcTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
             

                return command;
            }
            catch (OdbcException ex)
            {
                odbcTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
     
        #endregion
    }
}
