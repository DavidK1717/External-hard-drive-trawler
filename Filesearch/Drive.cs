using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Linq;
using MySql.Data;
using MySql.Data.MySqlClient;
using MySqlDB;


namespace Filesearch
{
    /// <summary>
    /// Hard drive class. When instantiated it writes a record to the database for the particulatr drive.
    /// </summary>
    public class Drive
    {
        public string VolumeLabel { get; private set; }

        public Drive(string driveLetter)
        {
            DriveInfo dv = new DriveInfo(driveLetter);
            VolumeLabel = dv.VolumeLabel;
            string totalCapacity = dv.TotalSize.ToString();
            string availableSpace = dv.AvailableFreeSpace.ToString();
            string fileSystem = dv.DriveFormat;
                      
            try
            {
                using (MySqlConnection cn = new MySqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    MySqlCommand cmd = new MySqlCommand();

                    cmd.CommandText = Convert.ToInt32(DataAccess.GetSingleValueSqlSP_stringParam("drive_check", VolumeLabel,
                                          "p_vol", 10, true)) > 0 ? "update_drive_record" : "create_drive_record";

                    cmd.Connection = cn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_vol",
                        MySqlDbType.VarChar, 10));
                    cmd.Parameters.Add(new MySqlParameter("p_total_cap",
                        MySqlDbType.Int64));
                    cmd.Parameters.Add(new MySqlParameter("p_available_space",
                        MySqlDbType.Int64));
                    cmd.Parameters.Add(new MySqlParameter("p_filesystem",
                        MySqlDbType.VarChar, 10));

                    cmd.Parameters[0].Value = VolumeLabel;
                    cmd.Parameters[1].Value = totalCapacity;
                    cmd.Parameters[2].Value = availableSpace;
                    cmd.Parameters[3].Value = fileSystem; 
                        
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]", "An error occurred");
            }
            
        }

        /// <summary>
        /// Writes folder and file counts to database.
        /// </summary>
        /// <param name="folderCount"></param>
        /// <param name="fileCount"></param>
        /// <returns></returns>
        public bool UpdateCounts(int folderCount, int fileCount)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "update_counts",
                        Connection = cn,
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new MySqlParameter("p_vol",
                        MySqlDbType.VarChar, 10));
                    cmd.Parameters.Add(new MySqlParameter("p_folder_count",
                        MySqlDbType.Int64));
                    cmd.Parameters.Add(new MySqlParameter("p_file_count",
                        MySqlDbType.Int64));
                    
                    cmd.Parameters[0].Value = VolumeLabel;
                    cmd.Parameters[1].Value = folderCount;
                    cmd.Parameters[2].Value = fileCount;
                    
                    cmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch (MySqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]", "An error occurred");
                return false;
            }
        }

    }  
    
    /// <summary>
    /// This is the class that does the main work.
    /// </summary>
    public class RootFolder
    {
        public string FolderPath { get; private set; }
        public string DriveLetter { get; private set; }
        private readonly Drive drive;
        private int folderCount = 0;
        private int fileCount = 0;

        /// <summary>
        /// New Drive object is instantiated in the constructor.
        /// </summary>
        /// <param name="folderPath"></param>
        public RootFolder(string folderPath)
        {
            FolderPath = folderPath;
            DriveLetter = FolderPath.Left(3);
            drive = new Drive(DriveLetter);
        }

        /// <summary>
        /// Deletes any existing file records for the current drive and then saves to the 
        /// database details of files in the root directory. Then calls the recursive 
        /// method that will trawl the entire directory structure of the drive.
        /// </summary>
        /// <returns></returns>
        public bool GetFiles()
        {
            try
            { 
                using (MySqlConnection cn = new MySqlConnection(DataAccess.connectionString))
                {
                    cn.Open();

                    // first delete any existing records for this drive
                    string strSQL = "delete from file where volume_label ='" + drive.VolumeLabel + "'";
                    
                    if (DataAccess.ExecuteSql(strSQL))
                    {
                        MySqlCommand cmd = new MySqlCommand("create_file_record", cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new MySqlParameter("p_volume",
                            MySqlDbType.VarChar));
                        cmd.Parameters.Add(new MySqlParameter("p_path",
                            MySqlDbType.VarChar));
                        cmd.Parameters.Add(new MySqlParameter("p_file_name",
                            MySqlDbType.VarChar));
                        cmd.Parameters.Add(new MySqlParameter("p_file_ext",
                            MySqlDbType.VarChar));
                        cmd.Parameters.Add(new MySqlParameter("p_size",
                            MySqlDbType.Int64));
                       

                        // get files in top directory first
                        DirectoryInfo diRoot = new DirectoryInfo(FolderPath);

                        foreach (FileInfo fi in diRoot.GetFiles())
                        {
                           SaveFileRecord(cmd, drive.VolumeLabel, fi);
                           fileCount++;
                        }

                        // call recursive function
                        this.GetAllFoldersUnder(diRoot, 0, cmd);
                        
                        return drive.UpdateCounts(folderCount, fileCount);
                    }
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]", "An error occurred");
                return false;
            }
        }

        /// <summary>
        /// Recursive method that traverses the entire directory structure.
        /// </summary>
        /// <param name="diRoot"></param>
        /// <param name="indent"></param>
        /// <param name="cmd"></param>
        private void GetAllFoldersUnder(DirectoryInfo diRoot, int indent, MySqlCommand cmd)
        {
            try
            {
                // The if statement filters out any symbolic links or junctions which can cause an infinite loop.

                if ((diRoot.Attributes & FileAttributes.ReparsePoint)
                    != FileAttributes.ReparsePoint)
                {
                    foreach (DirectoryInfo di in diRoot.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(d =>
                        !d.Name.Equals("System Volume Information") && !d.Name.Equals("$RECYCLE.BIN")))
                    {
                        try
                        {
                            foreach (FileInfo fi in di.GetFiles())
                            {
                                SaveFileRecord(cmd, drive.VolumeLabel, fi);
                                fileCount++;
                            }
                            
                            Console.WriteLine("{0}{1}", new string(' ', indent), di.Name);

                            folderCount++;

                            this.GetAllFoldersUnder(di, indent + 2, cmd);
                        }
                        catch (UnauthorizedAccessException uae)
                        {
                            Console.WriteLine("Inner: " + uae.Message);
                            var message = uae.Message.GetTextInSingleQuotes();
                            SaveUae(message);
                        }
                    }

                }
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("Outer: " + uae.Message);
                var message = uae.Message.GetTextInSingleQuotes();
                SaveUae(message);
            }
        }

        /// <summary>
        /// Saves to the database the paths of any folders that caiuse an UnauthorizedAccessException.
        /// </summary>
        /// <param name="message"></param>
        private void SaveUae(string message)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "add_uae",
                        Connection = cn,
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new MySqlParameter("p_vol",
                        MySqlDbType.VarChar, 10));
                    cmd.Parameters.Add(new MySqlParameter("p_path",
                        MySqlDbType.VarChar, 1000));

                    cmd.Parameters[0].Value = drive.VolumeLabel;
                    cmd.Parameters[1].Value = message;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                DataAccess.SqlExceptionHandler(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]", "An error occurred");
            }
        }

        private static void SaveFileRecord(MySqlCommand cmd, string vol, FileInfo fi)
        {
            cmd.Parameters[0].Value = vol;
            cmd.Parameters[1].Value = fi.FullName;
            cmd.Parameters[2].Value = fi.Name;
            cmd.Parameters[3].Value = fi.Extension.Right(3);
            cmd.Parameters[4].Value = fi.Length;

            cmd.ExecuteNonQuery();
        }

        
    }

    public static class util
    {
       // extension methods for .Net string class

        public static string Left(this string str, int length)
        {
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string Right(this string str, int length)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(str))
            {
                //Set valid empty string as string could be null
                str = string.Empty;
            }
            else if (str.Length > length)
            {
                //Make the string no longer than the max length
                str = str.Substring(str.Length - length, length);
            }

            //Return the string
            return str;
        }

        public static string GetTextInSingleQuotes(this string str)
        {
            var start = str.IndexOf("'") + 1;
            var end = str.LastIndexOf("'") - start;

            if (start == -1)
                return "";
            else
            {
                return str.Substring(start, end);
            }
        }

        public static void ExceptionHandler(Exception ex, string title)
        {
            MessageBox.Show("[" + ex.ToString() + "]", title);
        }
    }
}
