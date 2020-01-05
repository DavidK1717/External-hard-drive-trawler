using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MySqlDB
{
    /// <summary>
    /// Class for interacting with MySQL databases.
    /// </summary>
    public class DataAccess
    {
        public static string connectionString = "server=localhost;user=mgs_user;database=filesearch;password=pa55word";

        public static string MessageBoxTitle = "Filesearch Loader";


        public static void SqlExceptionHandler(MySqlException ex)
        {
            string message = ex.ToString();
            
            MessageBox.Show(message, DataAccess.MessageBoxTitle);
        }


        public static object GetSingleValueSqlSP_stringParam(string spName, string param,
            string paramName, int size, bool nullToZero)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    MySqlCommand cmd = new MySqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter(paramName,
                        MySqlDbType.VarChar, size));
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
            catch (MySqlException Sqlex)
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
                using (MySqlConnection cn = new MySqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, cn);
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();

                    return true;
                }

            }
            catch (MySqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);
                return false;
            }
        }
    }
}
