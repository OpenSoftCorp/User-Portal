using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace MiSAP.Models
{
    class DALConfig
    {
        private SqlConnection Conn;
        private SqlTransaction sqlTransaction;
        private string objCon;
    }
}
