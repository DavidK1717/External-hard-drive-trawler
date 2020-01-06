using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DTS;
using System.IO;
using System.Text;
using System.Configuration;

namespace DK.DataAccess
{
	
	public class DataAccess
	{        		
		public static string connectionString;
		public static string MessageBoxTitle;
               

        public static void SaveException(string ex)
        {
            DataAccess.ExecuteStoredProc_stringParam("save_exception", ex, "@ex", 4000);
        }

        public static void ExceptionHandler(Exception ex, string title)
        {
            MessageBox.Show("[" + ex.ToString() + "]", title);
            DataAccess.SaveException(ex.ToString());
        }

        public static void SqlExceptionHandler(SqlException ex, string title)
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
            MessageBox.Show(message, title);
            DataAccess.SaveException(message);
        }


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
			MessageBox.Show(message, DataAccess.MessageBoxTitle);
            DataAccess.SaveException(message);
		}

        public static bool DoesTableExist(string database, string tablename)
        {
            string sql = "use " + database + " select * from sysobjects where id = "
            + "object_id(N'" + @tablename + "') and OBJECTPROPERTY(id, N'"
            + "IsUserTable') = 1";

            DataSet ds = GetDataSetSql(sql);

            if (ds.Tables[0].Rows.Count == 1)
                return true;
            else
                return false;
        }



		public static object GetSingleValueSqlSP(string spName)
		{
			try
			{
				using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
				{
					cn.Open();
					SqlCommand cmd = new SqlCommand(spName, cn);
					cmd.CommandType = CommandType.StoredProcedure;
				
					Object retval = cmd.ExecuteScalar();
					if (retval != null)
						return retval;
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
			catch(Exception ex)
			{
				MessageBox.Show("["	+ ex.ToString() + "]");
				return -1;
			}
		}

        public static bool DataTableToDelimited(DataTable dt, string filePath, string delim, int maxRowLen)
        {
            try
            {
                // create the file to write to
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    foreach (DataRow dr in dt.Rows)
                    {

                        StringBuilder builder = new StringBuilder(maxRowLen); // No row is bigger than this
                        foreach (object o in dr.ItemArray)
                        {

                            builder.Append(o.ToString());
                            builder.Append(delim);
                        }
                        builder.Remove(builder.Length - 1, 1);
                        sw.WriteLine(builder.ToString());
                    }
                    sw.Flush();
                    return true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }
        }

        public static object GetSingleValueSqlSP_intParam(string spName, int param, string paramName)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.Int));
                    cmd.Parameters[0].Value = param;

                    Object retval = cmd.ExecuteScalar();
                    if (retval != null)
                        return retval;
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

        public static object GetSingleValueSqlSP_stringParam(string spName, string param, 
            string paramName, int size, bool nullToZero)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
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
                    else
                        if (nullToZero)
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

		public static DataSet GetDataSetSqlSP(string spName)
		{
            try
            {
                using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
								
                    DataSet ds = new DataSet();
                    da.Fill(ds, "table1");
                    cn.Close();
                    return ds;
                }
            }
            catch (SqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);
				
                return null;
            }
    
		}

        public static DataSet GetDataSetSqlSP_intParam(string spName, int param, string paramName)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.Int));
                    cmd.Parameters[0].Value = param;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
								
                    DataSet ds = new DataSet();
                    da.Fill(ds, "table1");
                    cn.Close();
                    return ds;
                }
            }
            catch (SqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);
				
                return null;
            }
    
        }

        public static DataSet GetDataSetSqlSP_stringParam(string spName, string param, string paramName, int size)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.NVarChar, size));
                    cmd.Parameters[0].Value = param;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    DataSet ds = new DataSet();
                    da.Fill(ds, "table1");
                    cn.Close();
                    return ds;
                }
            }
            catch (SqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);

                return null;
            }

        }

        public static DataSet GetDataSetSqlSP_datetimeParam(string spName, DateTime param, string paramName)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.DateTime));
                    cmd.Parameters[0].Value = param;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    DataSet ds = new DataSet();
                    da.Fill(ds, "table1");
                    cn.Close();
                    return ds;
                }
            }
            catch (SqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);

                return null;
            }

        }

		public static DataSet GetDataSetSqlTableOrView(string tableOrView)
		{
			try
			{
		
				using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
				{
					cn.Open();
					SqlCommand cmd = new SqlCommand("select * from " + tableOrView, cn);
					cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
									
					DataSet ds = new DataSet();
					da.Fill(ds, "table1");
					cn.Close();
					return ds;
				}
			}
			catch (SqlException ex)
			{
				DataAccess.SqlExceptionHandler(ex);
				
				return null;
			}
		}

		public static DataSet GetDataSetSql(string sqlString)
		{
            try
            {
                
                using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
                {
                    cn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(sqlString, cn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "table1");
                    
                    return ds;
                }
            }
            catch(SqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);
                return null;
            }
            
		}
		
		public static object GetSingleValueSql(string sqlString)
		{
			try
			{
				using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
				{
					cn.Open();
					SqlCommand cmd = new SqlCommand(sqlString, cn);
					cmd.CommandType = CommandType.Text;
				
					return cmd.ExecuteScalar();
                    
				}
			}
			catch (SqlException Sqlex)
			{
				MessageBox.Show("Database problem: ["
					+ Sqlex.ToString() + "]");
				return -1;
			}
			catch(Exception ex)
			{
				MessageBox.Show("["	+ ex.ToString() + "]");
				return -1;
			}
		}

        public static DataSet GetDataSetODBC(string connectionString, string sqlString)
        {
            try
            {
                using (OdbcConnection cn = new OdbcConnection(connectionString))
                {
                    cn.Open();
                    
                    OdbcDataAdapter da = new OdbcDataAdapter(sqlString, cn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "table1");

                    return ds;
                }
            }
            catch (SqlException Sqlex)
            {
                MessageBox.Show("Database problem: ["
                    + Sqlex.ToString() + "]");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return null;
            }
        }

        public static object GetSingleValueODBC(string connectionString, string sqlString)
        {
            try
            {
                using (OdbcConnection cn = new OdbcConnection(connectionString))
                {
                    cn.Open();
                    OdbcCommand cmd = new OdbcCommand(sqlString, cn);
                    cmd.CommandType = CommandType.Text;

                    return cmd.ExecuteScalar();

                    //if (retval != null)
                    //    return retval;
                    //else
                    //    return null;
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

		public static bool ExecuteStoredProc(string sp)
		{
			try
			{
				using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
				{
					cn.Open();
					SqlCommand cmd = new SqlCommand(sp, cn);
							
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandTimeout = 600;

					cmd.ExecuteNonQuery();

					return true;
				}
			}
					
			catch (SqlException Sqlex)
			{
				MessageBox.Show("Database problem: ["
					+ Sqlex.ToString() + "]");
				return false;
                
			}
			catch(Exception ex)
			{
				MessageBox.Show("["	+ ex.ToString() + "]");
				return false;
			}
		}

        public static bool ExecuteStoredProc_intParam(string spName, int param, string paramName)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.Int));
                    cmd.Parameters[0].Value = param;

                    cmd.ExecuteNonQuery();

                    return true;
                }
            }

            catch (SqlException Sqlex)
            {
                MessageBox.Show("Database problem: ["
                    + Sqlex.ToString() + "]");
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }
        }

        public static bool ExecuteStoredProc_intParams2(string spName, int param1, string paramName1,
            int param2, string paramName2)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 7200;
                    cmd.Parameters.Add(new SqlParameter(paramName1,
                        SqlDbType.Int));
                    cmd.Parameters[0].Value = param1;
                    cmd.Parameters.Add(new SqlParameter(paramName2,
                        SqlDbType.Int));
                    cmd.Parameters[1].Value = param2;
                    cmd.ExecuteNonQuery();

                    return true;
                }
            }

            catch (SqlException Sqlex)
            {
                MessageBox.Show("Database problem: ["
                    + Sqlex.ToString() + "]");
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }
        }

        public static bool ExecuteStoredProc_intParamOut(string spName, ref int param, string paramName)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.Int));
                    cmd.Parameters[0].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    param = (int)cmd.Parameters[0].Value;

                    return true;
                }
            }

            catch (SqlException Sqlex)
            {
                MessageBox.Show("Database problem: ["
                    + Sqlex.ToString() + "]");
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }
        }

        public static bool ExecuteStoredProc_stringParam(string spName, string param, string paramName, int size)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.NVarChar, size));
                    cmd.Parameters[0].Value = param;

                    cmd.ExecuteNonQuery();

                    return true;
                }
            }

            catch (SqlException Sqlex)
            {
                MessageBox.Show("Database problem: ["
                    + Sqlex.ToString() + "]");
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }
        }

        public static bool ExecuteStoredProc_datetimeParam(string spName, DateTime param, string paramName)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.DateTime));
                    cmd.Parameters[0].Value = param;

                    cmd.ExecuteNonQuery();

                    return true;
                }
            }

            catch (SqlException Sqlex)
            {
                MessageBox.Show("Database problem: ["
                    + Sqlex.ToString() + "]");
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }
        }

		public static bool ExecuteSql(string sql)
		{
			try
			{
				using (SqlConnection cn = new SqlConnection (DataAccess.connectionString))
				{
					cn.Open();
					SqlCommand cmd = new SqlCommand(sql, cn);
					cmd.CommandType = CommandType.Text;

					cmd.ExecuteNonQuery();

					return true;
				}

			}
			catch(SqlException ex)
			{
				DataAccess.SqlExceptionHandler(ex);
				return false;
			}
		}

		public static void SetCombo(ComboBox cbo, DataSet ds)
		{
			try
			{
				
				if (ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow row in ds.Tables[0].Rows)
					{
						cbo.Items.Add(row[0]);
					}
					cbo.SelectedItem = cbo.Items[0];
					
				}
				else
					throw new Exception("Combo box dataset is empty.");	
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem: ["
					+ ex.ToString() + "]");
				
			}
			finally
			{
				if (ds != null) ds.Clear();
			}

		}

        public static void SetCombo(ComboBox cbo, DataSet ds, bool clearDataSetAfterUse)
        {
            try
            {

                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        cbo.Items.Add(row[0]);
                    }
                    cbo.SelectedItem = cbo.Items[0];

                }
                else
                    throw new Exception("Combo box dataset is empty.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem: ["
                    + ex.ToString() + "]");

            }
            finally
            {
                if (clearDataSetAfterUse)
                    if (ds != null) ds.Clear();
            }

        }


		public static void SetCombo(ComboBox cbo, DataSet ds, string FirstVal)
		{
            //try
            //{
				
				if (ds.Tables[0].Rows.Count > 0)
				{
					cbo.Items.Add(FirstVal);

					foreach (DataRow row in ds.Tables[0].Rows)
					{
						cbo.Items.Add(row[0]);
					}
					cbo.SelectedItem = cbo.Items[0];
					
                }
            //    else
            //        throw new Exception("Combo box dataset is empty.");	
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Problem: ["
            //        + ex.ToString() + "]");
				
            //}
            //finally
            //{
            //    if (ds != null) ds.Clear();
            //}

		}

		public static void SetComboBind(ComboBox cbo, DataSet ds)
		{
			try
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					cbo.ValueMember = ds.Tables[0].Columns[0].ColumnName;
					cbo.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
					cbo.DataSource = ds.Tables[0];
					cbo.SelectedItem = cbo.Items[0];
                                        
				}
				else
					throw new Exception("Dataset is empty!.");	
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem: ["
					+ ex.ToString() + "]");
				
			}
		}

        public static void SetComboBind(DataGridViewComboBoxColumn cbo, DataSet ds)
        {
            try
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    cbo.ValueMember = ds.Tables[0].Columns[0].ColumnName;
                    cbo.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cbo.DataSource = ds.Tables[0];
                    

                }
                else
                    throw new Exception("Dataset is empty!.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem: ["
                    + ex.ToString() + "]");

            }
        }

        
		public static void SetComboBindSimple(ComboBox cbo, DataSet ds)
		{
			try
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					cbo.ValueMember = ds.Tables[0].Columns[0].ColumnName;
					cbo.DisplayMember = ds.Tables[0].Columns[0].ColumnName;
					cbo.DataSource = ds.Tables[0];
					cbo.SelectedItem = cbo.Items[0];
                                        
				}
				else
					throw new Exception("Dataset is empty!.");	
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem: ["
					+ ex.ToString() + "]");
				
			}
		}

	}
	
	// source: http://support.microsoft.com/default.aspx?scid=kb;en-us;326145
	public class DataSetHelper
	{
		public DataSet ds;
		private System.Collections.ArrayList m_FieldInfo; 
		private string m_FieldList;
		private System.Collections.ArrayList GroupByFieldInfo; 
		private string GroupByFieldList;


		public DataSetHelper(ref DataSet DataSet)
		{
			ds = DataSet;
		}
		public DataSetHelper()
		{
			ds = null;
		}

		private class FieldInfo
		{
			public string RelationName;
			public string FieldName;	//source table field name
			public string FieldAlias;	//destination table field name
			public string Aggregate;
		}

		private void ParseFieldList(string FieldList, bool AllowRelation)
		{
			/*
			 * This code parses FieldList into FieldInfo objects  and then
			 * adds them to the m_FieldInfo private member
			 *
			 * FieldList systax:  [relationname.]fieldname[ alias], ...
			*/
			if (m_FieldList == FieldList) return;
			m_FieldInfo = new System.Collections.ArrayList();
			m_FieldList = FieldList;
			FieldInfo Field; string[] FieldParts; string[] Fields=FieldList.Split(',');
			int i;
			for (i=0; i<=Fields.Length-1; i++)
			{
				Field=new FieldInfo();
				//parse FieldAlias
				FieldParts = Fields[i].Trim().Split(' ');
				switch (FieldParts.Length)
				{
					case 1:
						//to be set at the end of the loop
						break;
					case 2:
						Field.FieldAlias=FieldParts[1];
						break;
					default:
						throw new Exception("Too many spaces in field definition: '" + Fields[i] + "'.");
				}
				//parse FieldName and RelationName
				FieldParts = FieldParts[0].Split('.');
				switch (FieldParts.Length)
				{
					case 1:
						Field.FieldName=FieldParts[0];
						break;
					case 2:
						if (AllowRelation==false)
							throw new Exception("Relation specifiers not permitted in field list: '" + Fields[i] + "'.");
						Field.RelationName = FieldParts[0].Trim();
						Field.FieldName=FieldParts[1].Trim();
						break;
					default:
						throw new Exception("Invalid field definition: " + Fields[i] + "'.");
				}
				if (Field.FieldAlias==null)
					Field.FieldAlias = Field.FieldName;
				m_FieldInfo.Add (Field);
			}
		}

		private void ParseGroupByFieldList(string FieldList)
		{
			/*
			* Parses FieldList into FieldInfo objects and adds them to the GroupByFieldInfo private member
			*
			* FieldList syntax: fieldname[ alias]|operatorname(fieldname)[ alias],...
			*
			* Supported Operators: count,sum,max,min,first,last
			*/
			if (GroupByFieldList == FieldList) return;
			GroupByFieldInfo = new System.Collections.ArrayList();
			FieldInfo Field; string[] FieldParts; string[] Fields = FieldList.Split(',');
			for (int i=0; i<=Fields.Length-1;i++)
			{
				Field = new FieldInfo();
				//Parse FieldAlias
				FieldParts = Fields[i].Trim().Split(' ');
				switch (FieldParts.Length)
				{
					case 1:
						//to be set at the end of the loop
						break;
					case 2:
						Field.FieldAlias = FieldParts[1];
						break;
					default:
						throw new ArgumentException("Too many spaces in field definition: '" + Fields[i] + "'.");
				}
				//Parse FieldName and Aggregate
				FieldParts = FieldParts[0].Split('(');
				switch (FieldParts.Length)
				{
					case 1:
						Field.FieldName = FieldParts[0];
						break;
					case 2:
						Field.Aggregate = FieldParts[0].Trim().ToLower();    //we're doing a case-sensitive comparison later
						Field.FieldName = FieldParts[1].Trim(' ', ')');
						break;
					default:
						throw new ArgumentException("Invalid field definition: '" + Fields[i] + "'.");
				}
				if (Field.FieldAlias==null)
				{
					if (Field.Aggregate==null)
						Field.FieldAlias=Field.FieldName;
					else
						Field.FieldAlias = Field.Aggregate + "of" + Field.FieldName;
				}
				GroupByFieldInfo.Add(Field);
			}
			GroupByFieldList = FieldList;
		}

		public DataTable CreateGroupByTable(string TableName, DataTable SourceTable, string FieldList)
		{
			/*
			 * Creates a table based on aggregates of fields of another table
			 *
			 * RowFilter affects rows before GroupBy operation. No "Having" support
			 * though this can be emulated by subsequent filtering of the table that results
			 *
			 *  FieldList syntax: fieldname[ alias]|aggregatefunction(fieldname)[ alias], ...
			*/
			if (FieldList == null)
			{
				throw new ArgumentException("You must specify at least one field in the field list.");
				//return CreateTable(TableName, SourceTable);
			}
			else
			{
				DataTable dt = new DataTable(TableName);
				ParseGroupByFieldList(FieldList);
				foreach (FieldInfo Field in GroupByFieldInfo)
				{
					DataColumn dc  = SourceTable.Columns[Field.FieldName];
					if (Field.Aggregate==null)
						dt.Columns.Add(Field.FieldAlias, dc.DataType, dc.Expression);
					else
						dt.Columns.Add(Field.FieldAlias, dc.DataType);
				}
				if (ds != null)
					ds.Tables.Add(dt);
				return dt;
			}
		}

		public void InsertGroupByInto(DataTable DestTable, DataTable SourceTable, string FieldList,
			string RowFilter, string GroupBy)
		{
			/*
			 * Copies the selected rows and columns from SourceTable and inserts them into DestTable
			 * FieldList has same format as CreateGroupByTable
			*/
			if (FieldList == null)
				throw new ArgumentException("You must specify at least one field in the field list.");
			ParseGroupByFieldList(FieldList);	//parse field list
			ParseFieldList(GroupBy,false);			//parse field names to Group By into an arraylist
			DataRow[] Rows = SourceTable.Select(RowFilter, GroupBy);
			DataRow LastSourceRow = null, DestRow = null; bool SameRow; int RowCount=0;
			foreach(DataRow SourceRow in Rows)
			{
				SameRow=false;
				if (LastSourceRow!=null)
				{
					SameRow=true;
					foreach(FieldInfo Field in m_FieldInfo)
					{
						if (!ColumnEqual(LastSourceRow[Field.FieldName], SourceRow[Field.FieldName]))
						{
							SameRow=false;
							break;
						}
					}
					if (!SameRow)
						DestTable.Rows.Add(DestRow);
				}
				if (!SameRow)
				{
					DestRow = DestTable.NewRow();
					RowCount=0;
				}
				RowCount+=1;
				foreach(FieldInfo Field in GroupByFieldInfo)
				{
					switch(Field.Aggregate)    //this test is case-sensitive
					{
						case null:        //implicit last
						case "":        //implicit last
						case "last":
							DestRow[Field.FieldAlias]=SourceRow[Field.FieldName];
							break;
						case "first":
							if (RowCount==1)
								DestRow[Field.FieldAlias]=SourceRow[Field.FieldName];
							break;
						case "count":
							DestRow[Field.FieldAlias]=RowCount;
							break;
						case "sum":
							DestRow[Field.FieldAlias]=Add(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
							break;
						case "max":
							DestRow[Field.FieldAlias]=Max(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
							break;
						case "min":
							if (RowCount==1)
								DestRow[Field.FieldAlias]=SourceRow[Field.FieldName];
							else
								DestRow[Field.FieldAlias]=Min(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
							break;
					}
				}
				LastSourceRow = SourceRow;
			}
			if(DestRow!=null)
				DestTable.Rows.Add(DestRow);
		}


		private FieldInfo LocateFieldInfoByName(System.Collections.ArrayList FieldList, string Name)
		{
			//Looks up a FieldInfo record based on FieldName
			foreach (FieldInfo Field in FieldList)
			{
				if (Field.FieldName==Name)
					return Field;
			}
			return null;
		}

		private bool ColumnEqual(object a, object b)
		{
			/*
			* Compares two values to see if they are equal. Also compares DBNULL.Value.
			*
			* Note: If your DataTable contains object fields, you must extend this
			* function to handle them in a meaningful way if you intend to group on them.
			*/
			if ((a is DBNull) && (b is DBNull))
				return true; //both are null
			if ((a is DBNull) || (b is DBNull))
				return false; //only one is null

			if ((a is IComparable) && (b is IComparable) && (a.GetType() == b.GetType()))
			{
				if (((IComparable)a).CompareTo(b) == 0)
					return true;
				else
					return false;
			}
			return (a==b); //value type standard comparison
		}

		private object Min(object a, object b)
		{
			//Returns MIN of two values - DBNull is less than all others
			if ((a is DBNull) || (b is DBNull))
				return DBNull.Value;
			if (((IComparable)a).CompareTo(b)==-1)
				return a;
			else
				return b;
		}

		private object Max(object a, object b)
		{
			//Returns Max of two values - DBNull is less than all others
			if (a is DBNull)
				return b;
			if (b is DBNull)
				return a;
			if (((IComparable)a).CompareTo(b)==1)
				return a;
			else
				return b;
		}

		private object Add(object a, object b)
		{
			//Adds two values - if one is DBNull, then returns the other
			if (a is DBNull)
				return b;
			if (b is DBNull)
				return a;
			return ((decimal)a + (decimal)b);
		}

		public DataTable SelectGroupByInto(string TableName, DataTable SourceTable, string FieldList,
			string RowFilter, string GroupBy)
		{
			/*
			 * Selects data from one DataTable to another and performs various aggregate functions
			 * along the way. See InsertGroupByInto and ParseGroupByFieldList for supported aggregate functions.
			 */
			DataTable dt = CreateGroupByTable(TableName, SourceTable, FieldList);
			InsertGroupByInto(dt, SourceTable, FieldList, RowFilter, GroupBy);
			return dt;
		}


	}

	// This class loads and executes the DTS package.
	// Source: http://support.microsoft.com/?id=319985
	public class DTS_Package
	{
		
		/*	Prior to running this code, create a DTS package and save it to SQL Server. Then set a reference to
		//	the DTSPackage Object Library version 2.0 COM object.
		*/

		// must be set before use
		private string _serverName;
		
		private string _packageName;

		private Package2Class package;
        
		public DTS_Package(string name, string serverName)
		{
			_packageName = name;
			_serverName = serverName;
		}

		public bool Run()
		{
            
            package = new Package2Class();
            IConnectionPointContainer CnnctPtCont = (IConnectionPointContainer)package;

            IConnectionPoint CnnctPt;
            PackageEventsSink PES = new PackageEventsSink();
            Guid guid = new Guid("10020605-EB1C-11CF-AE6E-00AA004A34D5");  // UUID of PackageEvents Interface
            CnnctPtCont.FindConnectionPoint(ref guid, out CnnctPt);
            int iCookie;
            CnnctPt.Advise(PES, out iCookie);
            object pVarPersistStgOfHost = null;

            package.LoadFromSQLServer(this._serverName, null, null, DTSSQLServerStorageFlags.DTSSQLStgFlag_UseTrustedConnection, null,
                null, null, this._packageName, ref pVarPersistStgOfHost);

            package.Execute();
            package.UnInitialize();
            package = null;
            CnnctPt.Unadvise(iCookie); //a connection that is created by IConnectionPoint.Advise must be closed by calling IConnectionPoint.Unadvise to avoid a memory leak
            return true;
            
		}

	}

	//This class is responsible for handling DTS Package events. When an event is fired, a message is sent to
	//the console.
	public class PackageEventsSink : DTS.PackageEvents
	{

		public void OnQueryCancel(string EventSource, ref bool pbCancel)
		{
			Console.WriteLine("OnQueryCancel({0})", EventSource);
			pbCancel = false;
		}

		public void OnStart(string EventSource)
		{
			Console.WriteLine("OnStart({0})", EventSource);
		}

		public void OnProgress(string EventSource, string ProgressDescription, int PercentComplete, int ProgressCountLow, int ProgressCountHigh)
		{
			Console.WriteLine("OnProgress({0}, {1}, {2}, {3}, {4})", EventSource, ProgressDescription,
				PercentComplete, ProgressCountLow, ProgressCountHigh);
		}

		public void OnError(string EventSource, int ErrorCode, string Source, string Description, string HelpFile, int HelpContext, string

			IDofInterfaceWithError, ref bool pbCancel)
		{
			string err = String.Format("OnError({0}, {1}, {2}, {3}, {4}, {5})", EventSource, ErrorCode, Source, Description,
				HelpFile, HelpContext);
			MessageBox.Show(err, "DTS Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

			Console.WriteLine("OnError({0}, {1}, {2}, {3}, {4}, {5})", EventSource, ErrorCode, Source, Description,
				HelpFile, HelpContext);
			pbCancel = false;
		}

		public void OnFinish(string EventSource)
		{
			Console.WriteLine("OnFinish({0})", EventSource);
		}

	}

    public class DataSetEx : DataSet
    {
        public DataSetEx() { }

        public DataSetEx(string spName, int param, string paramName)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.Int));
                    cmd.Parameters[0].Value = param;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                                        
                    da.Fill(this, "table1");
                    cn.Close();
                    
                }
            }
            catch (SqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);

            }
        }

        public bool DataTableToDelimited(int index, string filePath, string delim, int maxRowLen)
        {
            try
            {
                // create the file to write to
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    foreach (DataRow dr in this.Tables[index].Rows)
                    {

                        StringBuilder builder = new StringBuilder(maxRowLen); // No row is bigger than this
                        foreach (object o in dr.ItemArray)
                        {

                            builder.Append(o.ToString());
                            builder.Append(delim);
                        }
                        builder.Remove(builder.Length - 1, 1);
                        sw.WriteLine(builder.ToString());
                    }
                    sw.Flush();
                    return true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }
        }
        /// <summary>
        /// Converts a given delimited file into a datatable within the dataset. 
        /// Assumes that the first line    
        /// of the text file contains the column names.
        /// </summary>
        /// <see cref="http://www.codeproject.com/cs/database/DataSetFrmDelimTxt.asp"/>
        /// <param name="File">The name of the file to open</param>    
        /// <param name="TableName">The name of the 
        /// Table to be made within the DataSet</param>
        /// <param name="delimiter">The string to delimit by</param>
        /// <returns>bool</returns>  
        public bool ImportTextFile(string File, string TableName, string delimiter)
        {
            try
            {
                //Open the file in a stream reader.
                StreamReader s = new StreamReader(File);

                //Split the first line into the columns       
                string[] columns = s.ReadLine().Split(delimiter.ToCharArray());

                //Add the new DataTable to the RecordSet
                this.Tables.Add(TableName);

                //Cycle the colums, adding those that don't exist yet 
                //and sequencing the one that do.
                foreach (string col in columns)
                {
                    bool added = false;
                    string next = "";
                    int i = 0;
                    while (!added)
                    {
                        //Build the column name and remove any unwanted characters.
                        string columnname = col + next;
                        columnname = columnname.Replace("#", "");
                        columnname = columnname.Replace("'", "");
                        columnname = columnname.Replace("&", "");

                        //See if the column already exists
                        if (!this.Tables[TableName].Columns.Contains(columnname))
                        {
                            //if it doesn't then we add it here and mark it as added
                            this.Tables[TableName].Columns.Add(columnname);
                            added = true;
                        }
                        else
                        {
                            //if it did exist then we increment the sequencer and try again.
                            i++;
                            next = "_" + i.ToString();
                        }
                    }
                }

                //Read the rest of the data in the file.        
                string AllData = s.ReadToEnd();

                //Split off each row at the Carriage Return/Line Feed
                //Default line ending in most windows exports.  
                //You may have to edit this to match your particular file.
                //This will work for Excel, Access, etc. default exports.
                string[] rows = AllData.Split("\r\n".ToCharArray());

                //Now add each row to the DataTable       
                foreach (string r in rows)
                {
                    //Split the row at the delimiter.
                    string[] items = r.Split(delimiter.ToCharArray());

                    //Add the item
                    this.Tables[TableName].Rows.Add(items);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");
                return false;
            }

            
        }


    }

    public class DataTableEx : DataTable
    {
        public DataTableEx() { }

        // http://wessamzeidan.net/cs/blog/rss.aspx?CategoryID=1002
        public static DataTable FilterTable(DataTable dt, string filterString)
        {
            DataRow[] filteredRows = dt.Select(filterString);
            DataTable filteredDt = dt.Clone();

            DataRow dr;
            foreach (DataRow oldDr in filteredRows)
            {
                dr = filteredDt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                    dr[dt.Columns[i].ColumnName] = oldDr[dt.Columns[i].ColumnName];
                filteredDt.Rows.Add(dr);

            }

            return filteredDt;
        }

        public static DataTable SortTable(DataTable dt, string sort)
        {
            DataRow[] sortedRows = dt.Select("1=1", sort);
            DataTable sortedDt = dt.Clone();

            DataRow dr;
            foreach (DataRow oldDr in sortedRows)
            {
                dr = sortedDt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                    dr[dt.Columns[i].ColumnName] = oldDr[dt.Columns[i].ColumnName];
                sortedDt.Rows.Add(dr);

            }

            return sortedDt;
        }
        // need to test if this gives same result as SortTable
        public static DataTable SortTable2(DataTable dt, string sort)
        {
            DataRow[] sortedRows = dt.Select("1=1", sort);
            DataTable sortedDt = dt.Clone();
                        
            foreach (DataRow dr in sortedRows)
            {
                sortedDt.Rows.Add(dr);
            }

            return sortedDt;
        }

        // http://www.netomatix.com/ViewToTable.aspx THIS METHOD DOES NOT WORK
        //public static DataTable DataViewToDataTable(DataView obDataView)
        //{
        //    if (null == obDataView)
        //    {
        //        throw new ArgumentNullException
        //        ("DataView", "Invalid DataView object specified");
        //    }

        //    DataTable obNewDt = obDataView.Table.Clone();
        //    int idx = 0;
        //    string[] strColNames = new string[obNewDt.Columns.Count];
        //    foreach (DataColumn col in obNewDt.Columns)
        //    {
        //        strColNames[idx++] = col.ColumnName;
        //    }

        //    IEnumerator viewEnumerator = obDataView.GetEnumerator();
        //    while (viewEnumerator.MoveNext())
        //    {
        //        DataRowView drv = (DataRowView)viewEnumerator.Current;
        //        DataRow dr = obNewDt.NewRow();
        //        try
        //        {
        //            foreach (string strName in strColNames)
        //            {
        //                dr[strName] = drv[strName];
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Trace.WriteLine(ex.Message);
        //        }
        //        obNewDt.Rows.Add(dr);
        //    }

        //    return obNewDt;
        //}					
    }

    

   
}
