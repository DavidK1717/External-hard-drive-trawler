using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Filesearch
{
    public partial class frmSelectDrives : Form
    {
        public frmSelectDrives()
        {
            InitializeComponent();
        }

        private void lblSelect_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            if (txtDrive.Text.Length == 0)
                txtDrive.Text = folderBrowserDialog1.SelectedPath;
            else
                txtDrive.Text = txtDrive.Text + Environment.NewLine + folderBrowserDialog1.SelectedPath;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            bool abortFlag = false;
            string abortFolder = "";
            string[] rootfolders = Regex.Split(txtDrive.Text, Environment.NewLine);

            try
            {
                if (txtDrive.Text.Length > 0)
                {
                    if (Dialogs.YesNo("proceed with import?",
                        "File Search"))
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        // To avoid a context swith deadlock (see: https://earlybites.wordpress.com/2012/04/10/resolution-context-switch-deadlock-was-detected/)
                        // we are using a seperate thread for the main operations.

                        var bw = new BackgroundWorker();

                        // define the event handlers

                        bw.DoWork += (sender1, args) =>
                        {
                            // this will happen in a separate thread

                            for (int i = 0; i < rootfolders.Length; i++)
                            {
                                RootFolder rf = new RootFolder(rootfolders[i]);

                                if (!rf.GetFiles())
                                {
                                    abortFlag = true;
                                    abortFolder = rootfolders[i];
                                }
                            }
                        };

                        bw.RunWorkerCompleted += (sender1, args) =>
                        {
                            if (args.Error != null) // if an exception occurred during DoWork,
                            {
                                MessageBox.Show(args.Error.ToString(), "File Search", 
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if(abortFlag)
                            {
                                MessageBox.Show("There was a problem reading " + 
                                                abortFolder + " - import aborted", "File Search",
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Import completed", "File Search",
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        };

                        bw.RunWorkerAsync(); // starts the background worker 
                    }
                }
                else
                    MessageBox.Show("Please select a drive.",
                        "File Search", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show("[" + ex.ToString() + "]");

            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
    }

    public static class Dialogs
    {
        public static bool YesNo(string message, string title)
        {
            if (MessageBox.Show(message, title,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                return true;
            else
                return false;
        }
    }
}
