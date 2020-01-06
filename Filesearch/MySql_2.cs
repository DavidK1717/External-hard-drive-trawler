using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySql_2
{
    public class DataAccess
    {
        public static string connectionString = "server=" + "DESKTOP-7UJF7DE" +
        ";Trusted_Connection=yes; database=" + "FileSearch";

        public static string MessageBoxTitle = "Filesearch Loader";


        public static void SqlExceptionHandler(SqlException ex)
        {
            string message = ex.ToString();

            if (ex.Errors.Count == 1)
            {
                if (!ex.Errors[0].Procedure.Equals(string.Empty))
                {
                    message += "\n\nStored procedure: " + ex.Errors[0].Procedure
                                                        + "\nLine number: " + ex.Errors[0].LineNumber;
                }
            }
            else
            {
                for (int x = 0; x < ex.Errors.Count; x++)
                {
                    message += "\n\nSQL Server Error " + x.ToString() +
                               "\n---------------\n";
                    if (!ex.Errors[x].Procedure.Equals(string.Empty))
                    {
                        message += "Stored procedure: " + ex.Errors[x].Procedure +
                                   "\nLine number: " + ex.Errors[x].LineNumber + "\n";
                    }

                    message += "Error message: " + ex.Errors[x];
                }
            }

            MessageBox.Show(message, SqlServerDB.DataAccess.MessageBoxTitle);
        }


        public static object GetSingleValueSqlSP_stringParam(string spName, string param,
            string paramName, int size, bool nullToZero)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(SqlServerDB.DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.NVarChar, size));
                    cmd.Parameters[0].Value = param;

                    Object retval = cmd.ExecuteScalar();
                    if (retval != null)
                        return retval;
                    else if (nullToZero)
                        return 0;
                    else
                        throw new Exception(spName + " did not return a value.");
                }
            }
            catch (SqlException Sqlex)
            {
                MessageBox.Show("Database problem: ["
                                + Sqlex.ToString() + "]");
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return -1;
            }
        }


        public static bool ExecuteSql(string sql)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(SqlServerDB.DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(sql, cn);
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();

                    return true;
                }

            }
            catch (SqlException ex)
            {
                SqlServerDB.DataAccess.SqlExceptionHandler(ex);
                return false;
            }
        }
    }
}
