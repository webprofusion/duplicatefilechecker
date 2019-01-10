using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DuplicateFileChecker
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void backgroundWorkerFileSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            //get list of file entries in location a
            string folderPath = txtFolderPath1.Text;
            List<FileEntry> fileList1 = new List<FileEntry>();
            GetFileList(folderPath, fileList1);

            System.Diagnostics.Debug.WriteLine("Processing 2nd Path:");
            List<FileEntry> fileList2 = new List<FileEntry>();
            GetFileList(txtFolderPath2.Text, fileList2);

            List<DuplicateEntry> duplicates = FindDuplicates(fileList1, fileList2);

            
            e.Result = duplicates;
        }

        private List<DuplicateEntry> FindDuplicates(List<FileEntry> fileList1, List<FileEntry> fileList2)
        {
            bool performHashCheck = true;
            List<DuplicateEntry> dupeList = new List<DuplicateEntry>();
            foreach (var file1 in fileList1)
            {
                var file2 = fileList2.FirstOrDefault(i => i.SizeBytes == file1.SizeBytes || i.FileName == file1.FileName || i.DateModified == file1.DateModified);
                if (file2 != null)
                {
                    DuplicateEntry entry = new DuplicateEntry();
                    entry.DuplicateStatus = DuplicateStatusType.None;

                    if (performHashCheck)
                    {
                        file1.FileHash = GetFileHash(file1.Path + "\\" + file1.FileName);
                        file2.FileHash = GetFileHash(file2.Path + "\\" + file2.FileName);
                    }

                    entry.FileEntry1 = file1;
                    entry.FileEntry2 = file2;

                    if (file1.FileName == file2.FileName && file1.SizeBytes == file2.SizeBytes) entry.DuplicateStatus = DuplicateStatusType.SameSizeAndName;
                    if ((file1.FileName == file2.FileName) && (file1.SizeBytes == file2.SizeBytes) && (file1.DateModified == file2.DateModified)) entry.DuplicateStatus = DuplicateStatusType.AppearsIdentical;
                    if (performHashCheck && (file1.FileHash == file2.FileHash)) entry.DuplicateStatus = DuplicateStatusType.ContentAppearsIdentical;

                    if (entry.DuplicateStatus != DuplicateStatusType.None)
                    {
                        dupeList.Add(entry);
                    }
                }
            }
            return dupeList;
        }

        private string GetFileHash(string pathName)
        {
            string strResult = "";
            string strHashData = "";

            byte[] arrbytHashValue;
            System.IO.FileStream oFileStream = null;

            System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher =
                       new System.Security.Cryptography.MD5CryptoServiceProvider();

            try
            {
                oFileStream = new System.IO.FileStream(pathName, System.IO.FileMode.Open,
                         System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);
                oFileStream.Close();

                strHashData = System.BitConverter.ToString(arrbytHashValue);
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error!",
                           System.Windows.Forms.MessageBoxButtons.OK,
                           System.Windows.Forms.MessageBoxIcon.Error,
                           System.Windows.Forms.MessageBoxDefaultButton.Button1);
            }

            return (strResult);
        }

        private void GetFileList(string path, List<FileEntry> fileList)
        {
            System.Diagnostics.Debug.WriteLine("Reading Files in: " + path);
            foreach (var filename in System.IO.Directory.GetFiles(path))
            {
                FileEntry f = new FileEntry();
                System.IO.FileInfo info = new System.IO.FileInfo(filename);
                f.FileName = info.Name.ToLower();
                f.Path = path;
                f.DateModified = info.LastWriteTime;
                f.SizeBytes = info.Length;
                fileList.Add(f);
            }

            foreach (var foldername in System.IO.Directory.GetDirectories(path))
            {
                GetFileList(foldername, fileList);
            }

        }
        private void backgroundWorkerFileSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorkerFileSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<DuplicateEntry> list = (List<DuplicateEntry>)e.Result;
            dataGridView1.DataSource = list;


            //.output csv
            string output = "Path1,Filename1,Size1,DateModified1, DupeType,Path2,Filename2,Size2,DateModified2\r\n";
            foreach (var entry in list)
            {
                output += entry.FileEntry1.Path + "," + entry.FileEntry1.FileName + "," + entry.FileEntry1.SizeBytes + "," + entry.FileEntry1.DateModified + "," + entry.DuplicateStatus.ToString() + "," + entry.FileEntry2.Path + "," + entry.FileEntry2.FileName + "," + entry.FileEntry2.SizeBytes + "," + entry.FileEntry2.DateModified + "\r\n";
            }
            System.IO.File.WriteAllText(txtOutputCSV.Text, output);

            Cursor.Current = Cursors.Default;

            MessageBox.Show(list.Count+" items found. CSV exported.");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(txtFolderPath1.Text) && System.IO.Directory.Exists(txtFolderPath2.Text))
            {
                Cursor.Current = Cursors.WaitCursor;
                this.progressBar1.Value = 10;
                backgroundWorkerFileSearch.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Can't find search folders - check folder paths 1 & 2.");
            }

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
