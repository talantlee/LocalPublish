using DataAccessLayers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalPublish
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.txt_basedif.Text = System.Configuration.ConfigurationManager.AppSettings.Get("ClientPublishDir");      
        }

        public void SaveConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var setUrl = config.AppSettings.Settings["ClientPublishDir"];
            if (setUrl == null)
            {
                config.AppSettings.Settings.Add("ClientPublishDir", this.txt_basedif.Text); ;
                config.Save(ConfigurationSaveMode.Modified);//保存
                ConfigurationManager.RefreshSection("appSettings");//刷新（防止已读入内存）
            }else
            {
                config.AppSettings.Settings["ClientPublishDir"].Value = this.txt_basedif.Text;
                config.Save(ConfigurationSaveMode.Modified);//保存
                ConfigurationManager.RefreshSection("appSettings");//刷新（防止已读入内存）
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.txt_basedif.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.txt_basedif.Text.Length > 3)
            {
                SaveConfig();
            }
        }
        string currentVersion = string.Empty;
        List<ReleaseFileInfo> needUpdateFiles = new List<ReleaseFileInfo>();
        private void button3_Click(object sender, EventArgs e)
        {
            SqlHelper db = DatabaseFactory.CreateDatabase();

            string sqlCommand = "select * from AssemblyInfo order by fileDate desc;";

            sqlCommand= "select Max(Version) from versions where islivied = 1 ";
           
             currentVersion = db.ExecuteScalarSqlString(sqlCommand).ToString();
            this.lbl_vertify.Text = $"當前版本號為：{currentVersion}";
            sqlCommand = "select * from AssemblyInfo order by fileDate desc;";

            DataTable olddata= db.ExecuteDatasetSqlString(sqlCommand).Tables[0];
            string[] newFileList=  System.IO.Directory.GetFiles(this.txt_basedif.Text, "*.*", System.IO.SearchOption.AllDirectories);
         
            this.listView1.Items.Clear();

            List<ReleaseFileInfo> newFileData = new List<ReleaseFileInfo>();
            foreach (string f in newFileList)
            {
                if (f.IndexOf(".pdb") > -1) continue;
                if (f.IndexOf(".scc") > -1) continue;
                if (f.IndexOf(".db") > -1) continue;
                if (f.IndexOf("\\logs\\") > -1) continue;
                if (f.IndexOf("\\data\\UserSet\\") > -1) continue;
                FileInfo fi = new FileInfo(f);
                ReleaseFileInfo file = new ReleaseFileInfo(f,f.Replace(this.txt_basedif.Text+"\\", ""), fi.Name, fi.LastWriteTime.Ticks);
                newFileData.Add(file);
             
            }
             needUpdateFiles = new List<ReleaseFileInfo>();
            foreach (ReleaseFileInfo fi in newFileData)
            {
                bool isFined = false;
                foreach(DataRow dr in olddata.Rows)
                {
                    if (dr["AssemblyPath"].ToString().Equals(fi.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                       
                        if (Convert.ToInt64(dr["FileDate"])==fi.FileDate)
                        {
                            isFined = true;
                        }
                        break;
                    }
                }
                if (!isFined)
                {
                    needUpdateFiles.Add(fi);
                  
                }
            }
            foreach (DataRow dr in olddata.Rows)
            {
                bool isFined = false;
                //isDelete
                foreach (ReleaseFileInfo fi in newFileData)
                {
                    if (dr["AssemblyPath"].ToString().Equals(fi.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        isFined = true;
                        break;
                    }
                }
                if (!isFined)
                {
                    ReleaseFileInfo file = new ReleaseFileInfo(this.txt_basedif.Text + "\\"+dr["AssemblyPath"].ToString(),dr["AssemblyPath"].ToString(), dr["AssemblyName"].ToString(), Convert.ToInt64(dr["FileDate"]));
                    file.isDeleted = true;
                    needUpdateFiles.Add(file);
                    // this.listView1.Items.Add(new ListViewItem(new string[] { fi.FileName, fi.FileDate.ToString() }));
                }
            }
            foreach (ReleaseFileInfo fi in needUpdateFiles)
            {
                this.listView1.Items.Add(new ListViewItem(new string[] { fi.FilePath, fi.FileDate.ToString(),fi.isDeleted?"delete":"change" }));
            }
            if (needUpdateFiles.Count == 0)
            {
                this.lbl_vertify.Text = "已經是最新版本。";
            }
                //       SYS_GetNeedUpdateFiles  return db.ExecuteDatasetSqlString(sqlCommand).Tables[0];
            }

        public class ReleaseFileInfo
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string TrueFilePath { get; set; }
            public long FileDate { get; set; }

            public bool isDeleted { get; set; }
            public ReleaseFileInfo(string fullpath,string fpath,string fname,long fdate)
            {
                FilePath = fpath;
                FileName = fname;
                FileDate = fdate;
                isDeleted = false;
                TrueFilePath = fullpath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (needUpdateFiles.Count > 0)
            {
                if (MessageBox.Show("確定要發佈新版本嗎？") == DialogResult.OK)
                {
                    this.progressBar1.Value = 0;
                    SqlHelper db = DatabaseFactory.CreateDatabase();
                    using (IDbConnection connection = db.GetConnection())
                    {
                        connection.Open();
                        IDbTransaction tran = connection.BeginTransaction();
                        try
                        {
                            foreach (ReleaseFileInfo fi in needUpdateFiles)
                            {
                                object[] para = { "new",fi.FileName, fi.FilePath,fi.FileDate,fi.isDeleted };
                                db.ExecuteNonQuery(tran, "SYS_AddNeedUpdateFile", para).ToString();
                                if(this.progressBar1.Value<98)
                                this.progressBar1.Value += 1;
                            }

                          string  newVsersion= db.ExecuteScalar(tran, "SYS_RefreshUpdateVersion", currentVersion).ToString();

                           this.lbl_vertify.Text = this.lbl_vertify.Text = $"已經更新，當前版本號為：{newVsersion}";


                          tran.Commit();
                          this.progressBar1.Value = 100;
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }

                    }
                    //  [SYS_AddNeedUpdateFile]
                    //1 upload to ftp
                    //2 update to database
                }
            }
            else
            {
                MessageBox.Show("沒檢測到任何文件的變動。");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (stp != null)
            {
                if (stp.IsAlive)
                    stp.Abort();
            }
            stp = new System.Threading.Thread(new System.Threading.ThreadStart(UploadToServer));
            stp.Start();

        }

        public void UploadToServer()
        {

            FtpClient ftp = new FtpClient("172.16.1.43", "tempftp", "tempftp123");
            this.progressBar1.Value = 0;
            int step = 100;
            if (needUpdateFiles.Count > 0)
                step = 100 / needUpdateFiles.Count;
            if (step == 0) step = 1;
            foreach (ReleaseFileInfo fi in needUpdateFiles)
            {
                this.lbl_vertify.Text = "正在上傳 "+fi.FilePath+" ...";
                System.Threading.Thread.Sleep(100);
                ftp.SubRootCount = 0;
                if (fi.FilePath.IndexOf("\\") > -1)
                {

                    ftp.PureUploadSub(fi.TrueFilePath, fi.FilePath);
                }
                else
                {
                    ftp.Upload(fi.TrueFilePath);
                }
                for (int i = 0; i < ftp.SubRootCount; i++)
                {
                    ftp.ChangeDir("..");
                }
                if (this.progressBar1.Value < 98)
                    this.progressBar1.Value += step;
            }
            progressBar1.Value = 100;
            this.lbl_vertify.Text = "上傳完成。";
        }


        System.Threading.Thread stp = null;
        private int CompairFileCount = 0;
        private void button6_Click(object sender, EventArgs e)
        {

            this.lbl_vertify.Text = "正在驗證 ...";
            if (stp != null)
            {
                if (stp.IsAlive)
                    stp.Abort();
            }
            stp = new System.Threading.Thread(new System.Threading.ThreadStart(Compair));
            stp.Start();
        }


        private void Compair()
        {
          
            FtpClient ftp = new FtpClient("172.16.1.43", "tempftp", "tempftp123");
            //ftp.get
            string[] newFileList = System.IO.Directory.GetFiles(this.txt_basedif.Text, "*.*", System.IO.SearchOption.AllDirectories);
            List<ReleaseFileInfo> newFileData = new List<ReleaseFileInfo>();
            foreach (string f in newFileList)
            {
                if (f.IndexOf(".pdb") > -1) continue;
                if (f.IndexOf(".scc") > -1) continue;
                if (f.IndexOf(".db") > -1) continue;
                if (f.IndexOf("\\logs\\") > -1) continue;
                if (f.IndexOf("\\data\\UserSet\\") > -1) continue;
                FileInfo fi = new FileInfo(f);
                ReleaseFileInfo file = new ReleaseFileInfo(f, f.Replace(this.txt_basedif.Text + "\\", ""), fi.Name, fi.LastWriteTime.Ticks);
                newFileData.Add(file);
            }
            CompairFileCount = 0;
            int step = 100;
            if (newFileData.Count > 0)
                step = 100 / newFileData.Count;
            if (step == 0) step = 1;

            foreach (ReleaseFileInfo fi in newFileData)
            {
                this.lbl_vertify.Text = "正在驗證 " + fi.FilePath + " ...";

                FileInfo file = new FileInfo(fi.TrueFilePath);
                long serversize = 0;
                try
                {
                    serversize = ftp.GetFileSize(fi.FilePath);
                }
                catch
                {
                    serversize = 0;
                }
                long localsize = file.Length;
                CompairFileCount++;
                if (serversize != localsize)
                {
                    System.Threading.Thread.Sleep(1000);
                    this.lbl_vertify.Text = fi.FilePath + " 文件不匹配，正在重新上傳...";
                    System.Threading.Thread.Sleep(2000);
                    ftp.SubRootCount = 0;
                    if (fi.FilePath.IndexOf("\\") > -1)
                    {

                        ftp.PureUploadSub(fi.TrueFilePath, fi.FilePath);
                    }
                    else
                    {
                        ftp.Upload(fi.TrueFilePath);
                    }
                    for (int i = 0; i < ftp.SubRootCount; i++)
                    {
                        ftp.ChangeDir("..");
                    }
                }
                if (this.progressBar1.Value < 98)
                    this.progressBar1.Value += step;

            }
            progressBar1.Value = 100;
            this.lbl_vertify.Text = "驗證完成。";
        }

     
    }
}
