using System;
using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;


namespace MiSAP.Models
{
    public class Dal_SQL
    {

        private SqlConnection Conn;
        private SqlTransaction sqlTransaction;
        private string objCon;
       


        IConfigurationBuilder builder = new ConfigurationBuilder();




        #region MiSAPOldDev

        public void data_proc_FileUpload(string procedureName, Hashtable rec)
        {
            try
            {
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommandForAttachment(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                Conn.Close();
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        public SqlCommand BuildQueryCommandForAttachment(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm = command.Parameters.Add("@returnValue", SqlDbType.Decimal);
                sprm.Direction = ParameterDirection.Output;



                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    if (myEnumerator.Key.ToString() == "FILE")
                    {
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@FILE", SqlDbType.Image);
                        pm.Value = myEnumerator.Value;
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString().Trim()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetRecord(DataSet ds)
        {
            SqlCommand cmd;
            SqlConnection con = OpenConnection();
            DataSet dsData = new DataSet();
            try
            {
                foreach (DataTable dt in ds.Tables)
                {
                    for (int index = 0; index < dt.Rows.Count; index++)
                    {
                        //Create New Command for each Proc.
                        cmd = new SqlCommand();
                        //Set Command Properties
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = dt.TableName;//Assign Proc Name
                        cmd.Connection = con;

                        //Add Parameter to the Command
                        cmd = AddCommandParameter(cmd, dt, index, false);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dsData);
                    }
                }

            }

            catch (Exception ex)
            {

            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            return dsData;
        }
        private SqlConnection Connect()
        {
            try
            {
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

                objCon = builder.Build().GetConnectionString("MiSAP");




                Conn = new SqlConnection(objCon);

                Conn.Open();
                sqlTransaction = Conn.BeginTransaction();
                return Conn;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
                //sqlTransaction.Rollback();
                //Conn.Close();
            }
        }

      
        public SqlConnection OpenConnection()
        {
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

            SqlConnection Con = new SqlConnection(builder.Build().GetConnectionString("MiSAP"));
            Con.Open();
            return Con;
        }
        public SqlCommand AddCommandParameter(SqlCommand cmd, DataTable dt, int index)
        {
            SqlParameter sprm = cmd.Parameters.Add("@returnValue", SqlDbType.Decimal);
            sprm.Direction = ParameterDirection.Output;

            for (int col = 0; col < dt.Columns.Count; col++)
            {
                if (dt.Columns[col].ColumnName == "timestamp")
                {
                    SqlParameter pm = new SqlParameter();
                    pm = cmd.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                    pm.Value = dt.Rows[index][col].ToString();
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@" + dt.Columns[col].ColumnName, dt.Rows[index][col].ToString()));
                }

            }
            return cmd;
        }
        public SqlCommand AddCommandParameter(SqlCommand cmd, DataTable dt, int index, bool ReturnVal)
        {
            if (ReturnVal)
            {
                SqlParameter pm = new SqlParameter("@returnValue", SqlDbType.NVarChar, 50);
                pm.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(pm);

            }
            for (int col = 0; col < dt.Columns.Count; col++)
            {
                if (dt.Columns[col].ColumnName.ToUpper() != "RETURNCOL")
                {
                    if (dt.Columns[col].ColumnName.ToUpper() == "TIMESTAMP")
                    {
                        SqlParameter pm = new SqlParameter();
                        pm = cmd.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = dt.Rows[index][col];
                    }
                    else
                    {
                        //cmd.Parameters.Add(new SqlParameter("@" + dt.Columns[col].ColumnName, dt.Rows[index][col].ToString()));
                        SqlParameter pm;
                        switch (dt.Columns[col].DataType.Name.ToUpper())
                        {
                            case "INTEGER":
                            case "INT32":
                                pm = new SqlParameter("@" + dt.Columns[col].ColumnName, SqlDbType.Int);
                                break;
                            case "BOOLEAN":
                                pm = new SqlParameter("@" + dt.Columns[col].ColumnName, SqlDbType.Bit);
                                break;
                            case "DATETIME":
                                pm = new SqlParameter("@" + dt.Columns[col].ColumnName, SqlDbType.DateTime);
                                break;
                            case "DOUBLE":
                                pm = new SqlParameter("@" + dt.Columns[col].ColumnName, SqlDbType.Decimal);
                                break;
                            default:
                                pm = new SqlParameter("@" + dt.Columns[col].ColumnName, SqlDbType.NVarChar, 255);
                                break;
                        }
                        pm.Value = dt.Rows[index][col];
                        cmd.Parameters.Add(pm);
                    }
                }
            }
            return cmd;
        }

        public string InsertRecord(DataSet ds)
        {
            SqlCommand cmd;
            SqlConnection con = OpenConnection();
            SqlTransaction trn;
            trn = con.BeginTransaction();
            string RA = "0";
            int index = 0;
            string dtName = "";
            try
            {
                foreach (DataTable dt in ds.Tables)
                {
                    bool ReturnFlag = false;
                    string ReturnCol = "";
                    string result = "";
                    dtName = dt.TableName;
                    for (index = 0; index < dt.Rows.Count; index++)
                    {
                        //Create New Command for each Proc.
                        cmd = new SqlCommand();
                        //Set Command Properties
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = dt.TableName;//Assign Proc Name
                        cmd.Connection = con;
                        cmd.Transaction = trn;

                        //Check For ReturnValue                   
                        foreach (DataColumn dc in dt.Columns)
                            if (dc.ColumnName.ToUpper() == "RETURNCOL")
                            {
                                ReturnFlag = true;
                                ReturnCol = dt.Rows[index][dc.ColumnName].ToString();
                                break;
                            }
                        //Set Return Value
                        if (ReturnFlag && ReturnCol != "" && result != "")
                            dt.Rows[index][ReturnCol] = result;

                        //Add Parameter to the Command
                        cmd = AddCommandParameter(cmd, dt, index, ReturnFlag);

                        cmd.ExecuteNonQuery();
                        if (ReturnFlag)
                            result = (string)cmd.Parameters["@returnValue"].Value;
                        //Exit if Error Occured
                        if (result == "-1")
                        {
                            RA = "-1;" + dt.TableName + ";" + (index + 1).ToString();
                            trn.Rollback();
                            break;
                        }
                        else
                            RA = result;
                    }
                }
                trn.Commit();
            }

            catch (Exception ex)
            {
                RA = "-1;" + dtName + ";" + (index + 1).ToString();
                trn.Rollback();
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            return RA;
        }

        public DataSet ReadRecord(DataSet dsParameter, bool ReturnFlag)
        {
            SqlCommand cmd;
            SqlConnection con = OpenConnection();
            DataSet DsReturnData = new DataSet();
            int index = 0;
            string dtName = "";
            try
            {
                foreach (DataTable dt in dsParameter.Tables)
                {
                    //bool ReturnFlag = false;
                    string ReturnCol = "";
                    string result = "";
                    dtName = dt.TableName;
                    for (index = 0; index < dt.Rows.Count; index++)
                    {
                        //Create New Command for each Proc.
                        cmd = new SqlCommand();
                        //Set Command Properties
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = dt.TableName;//Assign Proc Name
                        cmd.Connection = con;

                        //Check For ReturnValue Field
                        if (ReturnCol == "")
                            foreach (DataColumn dc in dt.Columns)
                                if (dc.ColumnName.ToUpper() == "RETURNCOL")
                                {
                                    ReturnFlag = true;
                                    ReturnCol = dt.Rows[index][dc.ColumnName].ToString();
                                    break;
                                }
                        if (ReturnFlag && ReturnCol != "" && result != "")
                            dt.Rows[index][ReturnCol] = result;

                        //Add Parameter to the Command
                        cmd = AddCommandParameter(cmd, dt, index, ReturnFlag);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(DsReturnData);
                        if (ReturnFlag)
                            result = (string)cmd.Parameters["@returnValue"].Value;
                        //Exit if Error Occured
                        DsReturnData.DataSetName = result;//Get Error No.
                        if (result != "1")
                        {
                            DsReturnData.DataSetName += ";" + dt.TableName + ";" + (index + 1).ToString();
                            break;
                        }
                    }
                }

            }

            catch (Exception ex)
            {
                DsReturnData.Clear();
                DsReturnData.DataSetName = "-1;" + dtName + ";" + (index + 1).ToString();
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            return DsReturnData;
        }

        public void data_proc(string procedureName, Hashtable rec)
        {
            try
            {
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommand(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                Conn.Close();
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }



        public void BatchProcessing(string procedureName, ArrayList arrayLst,string Db)
        {
            try
            {
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommand(procedureName, ht);
                    cmd.ExecuteNonQuery();
                }
                sqlTransaction.Commit();
                Conn.Close();
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string StringBatchProcessingCSL(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("tran_no");
                        ht.Add("tran_no", result.ToString().Trim());
                    }

                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string StringBatchProcessingSingleSSC(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("tran_no");
                        ht.Add("tran_no", result.ToString().Trim());
                    }
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand BuildQryCmd(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 30);
                sprm.Direction = ParameterDirection.Output;

                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                }

                return command;
            }
            catch (SqlException Sqlex)
            {

                sqlTransaction.Rollback();
                Conn.Close(); throw Sqlex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public SqlCommand BuildQueryCommandString(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 30);
                sprm.Direction = ParameterDirection.Output;

                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString() == "Image")
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                    }
                    else if (myEnumerator.Key.ToString() == "timestamp")
                    {
                        //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }
                    else if (myEnumerator.Key.ToString() == "output")
                    {
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@output", SqlDbType.VarChar);
                        pm.Direction = ParameterDirection.Output;
                        pm.Value = myEnumerator.Value;
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string CheckChildOnDelete(string procedureName, Hashtable rec)
        {
            string result = null;
            try
            {

                Conn = Connect();
                SqlCommand command = BuildQryCmd(procedureName, rec);
                command.ExecuteNonQuery();
                sqlTransaction.Commit();
                if (Convert.IsDBNull(command.Parameters["@returnValue"].Value) == true)
                { result = null; }
                else { result = (string)command.Parameters["@returnValue"].Value; }
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public SqlDataReader RunSQLProcedure_Reader(string storedProcName, Hashtable rec)
        {
            SqlDataReader returnReader;
            try
            {
                SqlCommand command = new SqlCommand();
                Connect();
                command = BuildQueryCommand(storedProcName, rec);
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return returnReader;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public SqlDataReader RunSQLProcedure_Reader(string storedProcName)
        {
            SqlDataReader returnReader;
            try
            {
                SqlCommand command = new SqlCommand();
                Connect();
                command = BuildQueryCommand(storedProcName);
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return returnReader;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public decimal CheckChildBeforeDelete(string procedureName, Hashtable rec)
        {
            decimal result = 0;
            try
            {
                Conn = Connect();
                SqlCommand command = BuildQueryCommand(procedureName, rec);
                command.ExecuteNonQuery();
                sqlTransaction.Commit();
                if (Convert.IsDBNull(command.Parameters["@returnValue"].Value) == true)
                { result = 0; }
                else { result = (decimal)command.Parameters["@returnValue"].Value; }
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public decimal data_procInsert(string procedureName, Hashtable rec)
        {
            try
            {
                decimal result = 0;
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommand(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                result = (decimal)cmd.Parameters["@returnValue"].Value;
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public decimal data_procDecimal(string procedureName, Hashtable rec)
        {
            try
            {
                decimal result = 0;
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommand(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                { result = 0; }
                else
                    result = (decimal)cmd.Parameters["@returnValue"].Value;
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public SqlCommand BuildQueryCommand(string storedProcName)
        {
            try
            {
                SqlCommand command;
                command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand BuildQueryCommand_Custom_Report(string storedProcName, ArrayList rec)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("exec " + storedProcName + "");

                for (int i = 0; i < rec.Count; i++)
                {
                    sb.Append("'" + rec[i] + "'");
                    if (i != rec.Count - 1)
                        sb.Append(",");
                }
                SqlCommand command = new SqlCommand(sb.ToString(), Conn, sqlTransaction);

                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;


                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public SqlCommand BuildQueryCommand(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm = command.Parameters.Add("@returnValue", SqlDbType.Decimal);
                sprm.Direction = ParameterDirection.Output;

                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString().ToUpper() == "TIMESTAMP")
                    {
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }
                    else if (myEnumerator.Key.ToString().ToUpper() == "FILE")
                    {
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@FILE", SqlDbType.Image);
                        pm.Value = myEnumerator.Value;
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand BuildQueryCommand(string storedProcName, Hashtable rec, SqlConnection con)
        {
            SqlCommand command = new SqlCommand(storedProcName, con);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 0;
            SqlParameter sprm = command.Parameters.Add("@returnValue", SqlDbType.Decimal);
            sprm.Direction = ParameterDirection.Output;


            IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
            while (myEnumerator.MoveNext())
            {

                if (myEnumerator.Key.ToString() == "timestamp")
                {
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                    SqlParameter pm = new SqlParameter();
                    pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                    pm.Value = myEnumerator.Value;
                }
                else
                {

                    command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                }
            }
            return command;
        }
        public DataTable select_proc(string procedureName, ArrayList rec)
        {
            try
            {
                DataTable dt = new DataTable();
                Conn = Connect();
                SqlDataAdapter sqlDa = new SqlDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand_Custom_Report(procedureName, rec);
                sqlDa.Fill(dt);
                Conn.Close();
                return dt;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataTable select_proc(string procedureName, Hashtable rec)
        {
            try
            {
                DataTable dt = new DataTable();
                Conn = Connect();
                SqlDataAdapter sqlDa = new SqlDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand(procedureName, rec);
                sqlDa.Fill(dt);
                Conn.Close();
                return dt;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataTable select_procSAP(string procedureName, Hashtable rec)
        {
            try
            {
                DataTable dt = new DataTable();
                Conn = Connect();
                SqlDataAdapter sqlDa = new SqlDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand(procedureName, rec);
                sqlDa.Fill(dt);
                Conn.Close();
                return dt;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void data_proc_attachment(string procedureName, Hashtable rec)
        {
            try
            {
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommandForAttachment(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                Conn.Close();
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
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
                Conn = Connect();
                SqlDataAdapter sqlDa = new SqlDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand(procedureName, rec);
                sqlDa.Fill(ds);
                Conn.Close();
                return ds;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataTable select_proc(string procedureName)
        {
            try
            {
                DataTable dt = new DataTable();
                Conn = Connect();
                SqlDataAdapter sqlDa = new SqlDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand(procedureName);
                sqlDa.Fill(dt);
                Conn.Close();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataSet select_procNew(string procedureName, Hashtable rec)
        {
            try
            {
                DataSet ds = new DataSet();
                Conn = Connect();
                SqlDataAdapter sqlDa = new SqlDataAdapter();
                sqlDa.SelectCommand = BuildQueryCommand(procedureName, rec);
                sqlDa.Fill(ds);
                Conn.Close();
                return ds;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public string StringBatchProcessingLDM(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("tran_no");
                        ht.Add("tran_no", result.ToString().Trim());
                    }
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string StringBatchProcessingInvoice(string procedureName, ArrayList arrayLst)
        {

            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("tran_no");
                        ht.Add("tran_no", result.ToString().Trim());
                    }
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;

                    if (result == "0001")
                        break;

                }

                if (result == "0001")
                {
                    sqlTransaction.Rollback();
                    Conn.Close();
                }
                else
                {
                    sqlTransaction.Commit();
                    Conn.Close();

                }

                return result;

            }
            catch (SqlException ex)
            {

                sqlTransaction.Rollback();
                Conn.Close();
                throw ex;

            }
            catch (Exception ex)
            {
                Conn.Close();
                throw ex;

            }


        }


        public decimal DecimalBatchProcessing(string procedureName, ArrayList arrayLst)
        {
            try
            {
                decimal result = 0;
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommand(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    result = (decimal)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public decimal DecimalBatchProcessingInvoice(string procedureName, ArrayList arrayLst)
        {
            try
            {
                decimal result = 0;
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommand(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    result = (decimal)cmd.Parameters["@returnValue"].Value;
                    if (result == -10010)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }

                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;

            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string StringBatchProcessing(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                //string dec = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("receipt_no");
                        ht.Add("receipt_no", result.ToString().Trim());
                    }
                    SqlCommand cmd = BuildQueryCommandBatchReciept(procedureName, ht);

                    cmd.ExecuteNonQuery();

                    result = (string)cmd.Parameters["@returnValue"].Value;

                }
                sqlTransaction.Commit();
                Conn.Close();

                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string StringBatchProcessing(string procedureName, ArrayList arrayLst, ref int rowindex)
        {
            try
            {
                string result = "";
                //string dec = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("receipt_no");
                        ht.Add("receipt_no", result.ToString().Trim());
                    }

                    SqlCommand cmd = BuildQueryCommandBatchReciept(procedureName, ht);

                    cmd.ExecuteNonQuery();

                    rowindex++;
                    result = (string)cmd.Parameters["@returnValue"].Value;
                    //result = (string)cmd.Parameters["@output"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();

                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand BuildQueryCommandNew(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;

                SqlParameter sprmValue = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 30);
                sprmValue.Direction = ParameterDirection.Output;

                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString() == "timestamp")
                    {
                        //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string StringExecuteNonQuery(string procedureName, Hashtable ht, string DataBase)
        {
            SqlConnection Con = new SqlConnection();

            string result = "";
            try
            {
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

                Con = new SqlConnection(builder.Build().GetConnectionString("MiSAP"));
                Con.Open();
                SqlCommand cmd = BuildQueryCommandString(procedureName, ht, Con);
                cmd.ExecuteNonQuery();
                result = Convert.ToString(cmd.Parameters["@returnValue"].Value);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return result;
        }


        public string StringBatchProcessingGeneralNew(string procedureName, ArrayList arrayLst, string replaceParameter)
        {
            SqlConnection Con = new SqlConnection();

            string result = "";
            try
            {
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Con = new SqlConnection(builder.Build().GetConnectionString("MiSAP"));

                Con.Open();
                sqlTransaction = Con.BeginTransaction();

                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove(replaceParameter);
                        ht.Add(replaceParameter, result.ToString().Trim());
                    }
                    SqlCommand cmd = BuildQueryCommandBatchRecieptNew(procedureName, ht, Con, sqlTransaction);
                    cmd.ExecuteNonQuery();
                    result = (string)cmd.Parameters["@returnValue"].Value;

                }
                sqlTransaction.Commit();
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                result = "-1";
            }
            finally
            {
                Con.Close();
            }
            return result;
        }

        public SqlCommand BuildQueryCommandBatchRecieptNew(string storedProcName, Hashtable rec, SqlConnection Con, SqlTransaction Tran)
        {
            SqlCommand command = new SqlCommand(storedProcName, Con, Tran);
            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm1 = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 30);
                sprm1.Direction = ParameterDirection.Output;
                //SqlParameter sprm2 = command.Parameters.Add("@output", SqlDbType.VarChar, 30);
                //sprm2.Direction = ParameterDirection.Output;
                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString() == "Image")
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                    }
                    else if (myEnumerator.Key.ToString() == "timestamp")
                    {
                        //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }

                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }

            catch (Exception ex)
            {

            }
            return command;
        }

        public string StringBatchProcessingGeneral(string procedureName, ArrayList arrayLst, string replaceParameter)
        {
            try
            {
                string result = "";
                //string dec = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove(replaceParameter);
                        ht.Add(replaceParameter, result.ToString().Trim());
                    }

                    SqlCommand cmd = BuildQueryCommandBatchReciept(procedureName, ht);

                    cmd.ExecuteNonQuery();

                    result = (string)cmd.Parameters["@returnValue"].Value;

                }
                sqlTransaction.Commit();
                Conn.Close();

                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand BuildQueryCommandBatchReciept(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm1 = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 30);
                sprm1.Direction = ParameterDirection.Output;

                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString() == "Image")
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                    }
                    else if (myEnumerator.Key.ToString() == "timestamp")
                    {

                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }

                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand BuildQueryCommandBatchValidationReceipt(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;

                SqlParameter sprm1 = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 30);
                sprm1.Direction = ParameterDirection.Output;




                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    if (myEnumerator.Key.ToString() == "timestamp")
                    {
                        //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string StringBatchProcessingValidationReciept(string procedureName, ArrayList arrayLst)
        {
            try
            {
                // string result = "";
                string dec = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                int i = 1;
                while (alEnumerator.MoveNext())
                {

                    ht = (Hashtable)alEnumerator.Current;
                    SqlCommand cmd = BuildQueryCommandBatchValidationReceipt(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    dec = (string)cmd.Parameters["@returnValue"].Value;
                    if (dec != "1")
                    {

                        return dec + "," + Convert.ToString(i);
                        break;
                    }
                    i++;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return dec + ",0";
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close();
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string DeleteRecordReceipt(string procedureName, Hashtable rec)
        {
            string result = "";
            try
            {
                Conn = Connect();
                SqlCommand command = BuildQueryCommandNew(procedureName, rec);
                command.ExecuteNonQuery();

                result = (string)command.Parameters["@returnValue"].Value;

                if (result != "1")
                {
                    return result + ",0,";
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public string StringBatchProcessingFreshReciept(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                //string dec = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("receipt_no");
                        ht.Add("receipt_no", result.ToString().Trim());
                    }
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchReciept(procedureName, ht);

                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else { result = (string)cmd.Parameters["@returnValue"].Value; }

                }
                sqlTransaction.Commit();
                Conn.Close();

                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ////////////////////////////////////////
        public string StringBatchProcessingSingle(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string StringBatchProcessingIssue(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("issue_no");
                        ht.Add("issue_no", result.ToString().Trim());
                    }
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value.ToString().Trim();
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                throw ex;
            }
        }

        public string StringBatchProcessingSTIIssue(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("issue_no");
                        ht.Add("issue_no", result.ToString().Trim());
                    }
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value.ToString().Trim();

                    if (result == "0001")
                        break;

                }
                if (result == "0001")
                {
                    sqlTransaction.Rollback();
                    Conn.Close();
                }
                else
                {
                    sqlTransaction.Commit();
                    Conn.Close();

                }
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string StringBatchProcessingSTIInvoice(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("INVOICE_NO");
                        ht.Add("INVOICE_NO", result.ToString().Trim());
                    }
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value.ToString().Trim();

                    if (result == "0001")
                        break;

                }
                if (result == "0001")
                {
                    sqlTransaction.Rollback();
                    Conn.Close();
                }
                else
                {
                    sqlTransaction.Commit();
                    Conn.Close();

                }
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string StringBatchProcessingAudjustment(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("adjustment_no");
                        ht.Add("adjustment_no", result.ToString().Trim());
                    }
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public decimal DecimalBatchProcessing(string procedureName, ArrayList arrayLst, ref int rowindex)
        {
            try
            {
                decimal result = 0;
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommand(procedureName, ht);
                    rowindex++;
                    result = Convert.ToDecimal(cmd.ExecuteScalar());
                    result = (decimal)cmd.Parameters["@returnValue"].Value;

                    if (result.CompareTo(1) == -1)
                    {
                        break;
                    }
                }

                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string DeleteWithOutputParametr(string procedureName, Hashtable rec)
        {
            string result = null;
            try
            {

                Conn = Connect();
                SqlCommand command = BuildQryCmd(procedureName, rec);
                command.ExecuteNonQuery();
                sqlTransaction.Commit();
                if (Convert.IsDBNull(command.Parameters["@returnValue"].Value) == true)
                { result = null; }
                else { result = (string)command.Parameters["@returnValue"].Value; }
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public SqlCommand BuildQueryCommandBatchSingle(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm1 = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 30);
                sprm1.Direction = ParameterDirection.Output;
                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString() == "Image")
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                    }
                    else if (myEnumerator.Key.ToString() == "timestamp")
                    {
                        //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }

                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string data_procString(string procedureName, Hashtable rec)
        {
            try
            {
                string result = "";
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommandString(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                result = (string)cmd.Parameters["@returnValue"].Value;
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public void data_procwithoutResult(string procedureName, Hashtable rec)
        {
            try
            {
                string result = "";
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommandString(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                Conn.Close();
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public int data_procInsert1(string procedureName, Hashtable rec)
        {
            try
            {
                int result = 0;
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommand1(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                result = (int)cmd.Parameters["@returnValue"].Value;
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public SqlCommand BuildQueryCommand1(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm = command.Parameters.Add("@returnValue", SqlDbType.Int);
                sprm.Direction = ParameterDirection.Output;

                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString() == "timestamp")
                    {

                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public decimal data_procDecimalNew(string procedureName, Hashtable rec, ref int rowindex)
        {
            try
            {
                decimal result = 0;
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommand(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                { result = 0; }
                else
                    result = (decimal)cmd.Parameters["@returnValue"].Value;
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public string data_procString_ValidateORInsert(string procedureName, Hashtable rec)
        {
            try
            {
                string result = "";
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommandString(procedureName, rec);
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                { result = ""; }
                else
                    result = (string)cmd.Parameters["@returnValue"].Value;
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private String _connectionString;
        public String ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

                    _connectionString = builder.Build().GetConnectionString("MiSAP");
                }
                return _connectionString;
            }
        }


        public string StringBatchProcessingImport(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value.ToString().Trim();

                    if (result == "-1")
                        break;

                }
                if (result == "-1")
                {
                    sqlTransaction.Rollback();
                    Conn.Close();
                }
                else
                {
                    sqlTransaction.Commit();
                    Conn.Close();

                }
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /* From Here Function is for CRM*/
        public int data_procInt(string procedureName, Hashtable rec)
        {
            int result;
            try
            {

                Conn = Connect();
                SqlCommand cmd = BuildQueryCommand(procedureName, rec);
                result = cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
                Conn.Close();
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;

        }

        public string StringBatchProcessingWithoutResult(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    //command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public decimal DecimalBatchProcessingRef(string procedureName, ArrayList arrayLst, ref int rowindex)
        {
            try
            {
                decimal result = 0;
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    SqlCommand cmd = BuildQueryCommand(procedureName, ht);
                    result = Convert.ToDecimal(cmd.ExecuteScalar());
                    //result = (decimal)cmd.Parameters["@returnValue"].Value;
                    rowindex++;
                    if (result.CompareTo(1) == -1)
                    {
                        break;
                    }
                }

                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public string StringBatchProcessingCommon(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("gen_id");
                        ht.Add("gen_id", result.ToString().Trim());
                    }
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value;
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string StringBatchProcessingEnquiry(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("CALL_ID");
                        ht.Add("CALL_ID", result.ToString().Trim());
                    }
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value.ToString().Trim();
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string StringBatchProcessingReturn(string procedureName, ArrayList arrayLst)
        {
            try
            {
                string result = "";
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != "")
                    {
                        ht.Remove("RETURN_NO");
                        ht.Add("RETURN_NO", result.ToString().Trim());
                    }
                    SqlCommand cmd = BuildQueryCommandBatchSingle(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = ""; }
                    else
                        result = (string)cmd.Parameters["@returnValue"].Value.ToString().Trim();
                }
                sqlTransaction.Commit();
                Conn.Close();
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public int IntBatchProcessingGeneral(string procedureName, ArrayList arrayLst, string replaceParameter)
        {
            try
            {
                int result = -1;
                Hashtable ht = new Hashtable();
                IEnumerator alEnumerator = arrayLst.GetEnumerator();
                Conn = Connect();
                while (alEnumerator.MoveNext())
                {
                    ht = (Hashtable)alEnumerator.Current;
                    if (result != -1)
                    {
                        ht.Remove(replaceParameter);
                        ht.Add(replaceParameter, result);
                    }
                    SqlCommand cmd = BuildQueryCommandBatchInt(procedureName, ht);
                    cmd.ExecuteNonQuery();
                    if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                    { result = -1; }
                    else
                        result = (int)cmd.Parameters["@returnValue"].Value;

                    if (result == -1)
                        break;

                }
                if (result == -1)
                {
                    sqlTransaction.Rollback();
                    Conn.Close();
                }
                else
                {
                    sqlTransaction.Commit();
                    Conn.Close();

                }
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SqlCommand BuildQueryCommandBatchInt(string storedProcName, Hashtable rec)
        {
            try
            {
                SqlCommand command = new SqlCommand(storedProcName, Conn, sqlTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SqlParameter sprm1 = command.Parameters.Add("@returnValue", SqlDbType.Int);
                sprm1.Direction = ParameterDirection.Output;
                IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
                while (myEnumerator.MoveNext())
                {

                    if (myEnumerator.Key.ToString() == "Image")
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value));
                    }
                    else if (myEnumerator.Key.ToString() == "timestamp")
                    {
                        SqlParameter pm = new SqlParameter();
                        pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                        pm.Value = myEnumerator.Value;
                    }

                    else
                    {
                        command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                    }
                }

                return command;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int IntProcessingGeneral(string procedureName, Hashtable rec)
        {
            try
            {
                int result = 0;
                Conn = Connect();
                SqlCommand cmd = BuildQueryCommandBatchInt(procedureName, rec);
                cmd.ExecuteNonQuery();
                if (Convert.IsDBNull(cmd.Parameters["@returnValue"].Value) == true)
                { result = -1; }
                else
                    result = (int)cmd.Parameters["@returnValue"].Value;

                if (result == -1)
                {
                    sqlTransaction.Rollback();
                    Conn.Close();
                }
                else
                {
                    sqlTransaction.Commit();
                    Conn.Close();

                }
                return result;
            }
            catch (SqlException ex)
            {
                sqlTransaction.Rollback();
                Conn.Close(); throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public SqlCommand BuildQueryCommandString(string storedProcName, Hashtable rec, SqlConnection con)
        {
            SqlCommand command = new SqlCommand(storedProcName, con);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 0;
            SqlParameter sprm = command.Parameters.Add("@returnValue", SqlDbType.VarChar, 5000);
            sprm.Direction = ParameterDirection.Output;

            IDictionaryEnumerator myEnumerator = rec.GetEnumerator();
            while (myEnumerator.MoveNext())
            {

                if (myEnumerator.Key.ToString() == "timestamp")
                {
                    SqlParameter pm = new SqlParameter();
                    pm = command.Parameters.Add("@timestamp", SqlDbType.Timestamp);
                    pm.Value = myEnumerator.Value;
                }
                else
                {

                    command.Parameters.Add(new SqlParameter("@" + (myEnumerator.Key).ToString(), myEnumerator.Value.ToString()));
                }
            }
            return command;
        }
        public string StringExecuteNonQueryForPPC(string procedureName, Hashtable ht)
        {
            SqlConnection Con = new SqlConnection();

            string result = "";
            try
            {
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

                Con = new SqlConnection(builder.Build().GetConnectionString("MiSAP"));
                Con.Open();
                SqlCommand cmd = BuildQueryCommandString(procedureName, ht, Con);
                cmd.ExecuteNonQuery();
                result = Convert.ToString(cmd.Parameters["@returnValue"].Value);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {

                Con.Close();
            }
            return result;
        }

      


        #endregion
    }
}
