using DataAccessLayers;
using FluentFTP;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;
using AutoLocalPublish.Models;
using BusinessFacade;
using System.Runtime.ConstrainedExecution;

namespace AutoLocalPublish
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static List<FileAttr> exfileAttrs = new List<FileAttr>();
        string backupdir = string.Empty;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.txt_basedif.Text = System.Configuration.ConfigurationManager.AppSettings.Get("LocalPublishDir");

            exfileAttrs = (List<FileAttr>)System.Configuration.ConfigurationManager.GetSection("FileConfig");

        
            SqlHelper db = DatabaseFactory.CreateDatabase();

            string sqlCommand = "";

            sqlCommand = "select Max(Version) from versions where isLive = 1 ";

          

            currentVersion = db.ExecuteScalarSqlString(sqlCommand).ToString();
            if (AppConfig.BackUpDir.Length > 0)
            {
                if(AppConfig.BackUpDir.EndsWith("\\"))
                this.tbx_backupdir.Text = AppConfig.BackUpDir + currentVersion;
                else
                    this.tbx_backupdir.Text = AppConfig.BackUpDir +"\\"+ currentVersion;
            }
           
            if (AppConfig.PublishToDir.Length > 0)
            {
                if (AppConfig.PublishToDir.EndsWith("\\"))
                {
                    this.tbx_publishdir.Text = AppConfig.PublishToDir;
                }else
                {
                    this.tbx_publishdir.Text = AppConfig.PublishToDir+"\\";
                }
            }
                

    


            this.lbl_vertify.Text = $"當前版本號為：{currentVersion}";

        }
        private void BackUpVersions()
        {

        }

        public void SaveConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var setUrl = config.AppSettings.Settings["LocalPublishDir"];
            if (setUrl == null)
            {
                config.AppSettings.Settings.Add("LocalPublishDir", this.txt_basedif.Text); ;
                config.Save(ConfigurationSaveMode.Modified);//保存
                ConfigurationManager.RefreshSection("appSettings");//刷新（防止已读入内存）
            }
            else
            {
                config.AppSettings.Settings["LocalPublishDir"].Value = this.txt_basedif.Text;
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
        bool isbackupsuccess = false;
        private void button3_Click(object sender, EventArgs e)
        {
            BroadcastAutoId = 0;
            isbackupsuccess = false;
            needUpdateFiles = new List<ReleaseFileInfo>();
            if (this.tbx_backupdir.Text.Length == 0 || this.tbx_publishdir.Text.Length == 0)
            {
                MessageBox.Show("未設置更新/備份目錄。");
                return;
            }
                //TODO:1 根目錄 只能放 exe,dll,xml,.config,.json,.runtimeconfig,ico
                string[] basenewFileList = System.IO.Directory.GetFiles(this.txt_basedif.Text, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (string f in basenewFileList)
            {
                if (f.ToLower().IndexOf(".exe") > -1
                    || f.ToLower().IndexOf(".dll") > -1
                       || f.ToLower().IndexOf(".config") > -1
                        || f.ToLower().IndexOf(".xml") > -1
                          || f.ToLower().IndexOf(".json") > -1
                           || f.ToLower().IndexOf(".runtimeconfig") > -1
                            || f.ToLower().IndexOf(".ico") > -1
                             || f.ToLower().IndexOf(".db") > -1
                    )
                {
                    continue;
                }
                else
                {
                    MessageBox.Show("根目錄只能存放 exe,dll,xml,.config,.json,.runtimeconfig,ico 這些文件，請確認文件目錄是否有放錯。");
                    return;
                }
            }


            string[] newFileList = System.IO.Directory.GetFiles(this.txt_basedif.Text, "*.*", System.IO.SearchOption.AllDirectories);
            this.listView1.Items.Clear();
            List<ReleaseFileInfo> newFileData = new List<ReleaseFileInfo>();
            List<string> excludeFiles = new List<string>();
           
            List<string> notAllowUpdateFiles = new List<string>();
            //todo:除了runtimes 或根目錄，其他地方不允許放置dll,exe.
            List<string> excludeBaseDir = new List<string>();
            exfileAttrs.ForEach(attr => {
                if (attr.OpType.ToLower() == "exclude")
                {
                    excludeFiles.Add(attr.Key);
                }
                else if (attr.OpType.ToLower() == "allowsuddir")
                {
                    excludeBaseDir.Add(attr.Key);
                }
            });
         

            if (this.tbx_backupdir.Text.Length > 0 && this.tbx_publishdir.Text.Length>0)
            {
                foreach (string f in newFileList)
                {
                    if (f.IndexOf(".pdb") > -1) {continue;}

                    if (f.IndexOf(".scc") > -1) {  continue; }
                    if (f.IndexOf(".db") > -1) {  continue; }
                    if (f.IndexOf("\\ref\\") > -1) { continue; }
                    if (f.IndexOf("\\logs\\") > -1) {  continue; }
                    //   if (f.IndexOf("\\runtimes\\") > -1) {

                    //win-x64,win-x86,win-arm64,
                    //    continue;
                    //   }


                    if (f.IndexOf("\\StartUp.exe.WebView2\\") > -1) {  continue; }
                    if (f.IndexOf("\\NMERP.exe.WebView2\\") > -1) {  continue; }


                    if (f.IndexOf("\\WebView2Data\\EBWebView\\") > -1) { continue; }
                    if (f.IndexOf("\\WebView2Data\\tempfiles\\") > -1) {  continue; }
                    if (f.IndexOf(".deps.json") > -1) { continue; }//dagger.li 2023-12-20


                  
                    if (f.IndexOf("\\data\\UserSet\\") > -1) {  continue; }
                    if (f.IndexOf("\\updated\\") > -1) {  continue; }
                    if (f.IndexOf("Infragistics.") > -1 && f.IndexOf(".xml") > -1) { continue; }
                    if (f.IndexOf("defaultLoginer.xml") > -1) { continue; }
                
                    bool isexclude = false;
                    foreach (var item in excludeFiles)
                    {
                        if (f.ToLower().IndexOf(item.ToLower()) > -1)
                        {
                            isexclude = true;
                            break;
                        }
                    }
                    if (!isexclude)
                    {
                        if (f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            // //todo:除了runtimes 或根目錄，其他地方不允許放置dll,exe.
                            var subbasedir = f.Replace("/", "\\").Replace(this.txt_basedif.Text.Replace("/", "\\")+"\\", "");
                      
                            if (subbasedir.Contains("\\") && !subbasedir.StartsWith("runtimes", StringComparison.OrdinalIgnoreCase))
                            {
                                isexclude = true;
                                foreach (var item in excludeBaseDir)
                                {
                                    if (f.ToLower().IndexOf(item.ToLower()) > -1)
                                    {
                                        isexclude = false;
                                        break;
                                    }
                                }
                            }

                        }
                    }
                  
                    if (isexclude) { notAllowUpdateFiles.Add(f); continue; }
                    //  if (f.StartsWith("ErpUpdate.")) continue;

                    FileInfo fi = new FileInfo(f);

                    ReleaseFileInfo file = new ReleaseFileInfo(f, f.Replace(this.txt_basedif.Text + "\\", ""), fi.Name, fi.LastWriteTime.Ticks, fi.Length);
                    newFileData.Add(file);

                }

                //TODO: 禁止一些文件更新到客戶端。
                if(notAllowUpdateFiles.Count > 0)
                {
                    StringBuilder sb=new StringBuilder();
                    foreach (var f in notAllowUpdateFiles) { sb.Append(f.ToString()).AppendLine(); }
                    MessageBox.Show("這些文件不允許更新到客戶端，請確認：\n"+ sb.ToString());
                    return;
                }
             

                //backup files
                if (newFileData.Count > 0)
                {
                    if (!Directory.Exists(this.tbx_backupdir.Text))
                    {
                        Directory.CreateDirectory(this.tbx_backupdir.Text);
                    }
                 

                    //check backup files.
                    SqlHelper db = DatabaseFactory.CreateDatabase();
                    string sqlCommand = "";
                    sqlCommand = "select * from AssemblyInfo order by fileDate desc;";

                    DataTable olddata = db.ExecuteDatasetSqlString(sqlCommand).Tables[0];

                  

                    foreach (ReleaseFileInfo fi in newFileData)
                    {
 
                        foreach (DataRow dr in olddata.Rows)
                        {
                            if (dr["AssemblyPath"].ToString().Equals(fi.FilePath, StringComparison.OrdinalIgnoreCase))
                            {

                                if (Convert.ToInt64(dr["FileDate"]) == fi.FileDate)
                                {
                                    fi.isChanged = false;
                                   // isFined = true;
                                }
                                break;
                            }
                        }
                        needUpdateFiles.Add(fi);

                    }
                    foreach (ReleaseFileInfo fi in needUpdateFiles)
                    {
                        this.listView1.Items.Add(new ListViewItem(new string[] { fi.FilePath, fi.FileDate.ToString(), fi.isChanged ? "change" : "no change" }));
                    }
                    if(!needUpdateFiles.ToList<ReleaseFileInfo>().Any(att => att.isChanged))
                  //  if (needUpdateFiles.Count == 0)
                    {
                        this.lbl_vertify.Text = "沒有文件需要更新。";
                    }


                }
            }
            else
            {
                MessageBox.Show("未設置更新/備份目錄。");
            }
           
            //       SYS_GetNeedUpdateFiles  return db.ExecuteDatasetSqlString(sqlCommand).Tables[0];
        }

     
        int BroadcastAutoId = 0;
        string newVsersion = string.Empty;
        private void button4_Click(object sender, EventArgs e)
        {
            if (!needUpdateFiles.ToList<ReleaseFileInfo>().Any(att => att.isChanged))
            //  if (needUpdateFiles.Count == 0)
            {
                this.lbl_vertify.Text = "沒有文件需要更新。";
                return;
            }
            isbackupsuccess = false;
            try
            {
                foreach (ReleaseFileInfo fi in needUpdateFiles)
                {
                    if (fi.isChanged)
                    {
                        string todir = fi.TrueFilePath.Replace(this.txt_basedif.Text, this.tbx_backupdir.Text).Replace("/","\\");
                        todir= todir.Substring(0,todir.LastIndexOf("\\"));
                        if (!Directory.Exists(todir))
                        {
                            Directory.CreateDirectory(todir);
                        }
                        string oldfilepath= fi.TrueFilePath.Replace(this.txt_basedif.Text+"\\", this.tbx_publishdir.Text).Replace("/", "\\");
                        if (File.Exists(oldfilepath))
                        {
                            if(!File.Exists(todir + "\\" + fi.FileName))
                                System.IO.File.Copy(oldfilepath, todir + "\\" + fi.FileName, true);
                           
                        }
                    }
                }

            
                isbackupsuccess = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"back up error. {ex.Message}.");
                return;
            }
            if (!isbackupsuccess)
            {
                MessageBox.Show("請先備份當前版本的這些文件。");
                return;
            }
            if (needUpdateFiles.Count > 0)
            {
                BroadcastAutoId = 0;
                if (MessageBox.Show("確定要發佈新版本嗎？", "Tips", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    this.progressBar1.Value = 0;
                    SqlHelper db = DatabaseFactory.CreateDatabase();

                    newVsersion = string.Empty;
                    using (IDbConnection connection = db.GetConnection())
                    {
                        connection.Open();
                        IDbTransaction tran = connection.BeginTransaction();
                        try
                        {
                            newVsersion = db.ExecuteScalar(tran, "Versions_Edit", 0, "發佈器自動產生", "mis", "N", "updates").ToString();
                            if (newVsersion == "-1")
                            {
                                MessageBox.Show("已經有一個版本未上線，請先上線上一個版本 或 刪除上一個未上線版本，再繼續發佈新版。");
                                return;
                            }

                            foreach (ReleaseFileInfo fi in needUpdateFiles)
                            {
                                if(fi.isChanged)
                                {
                                    object[] para = { newVsersion, fi.FileName, fi.FilePath, fi.FileDate, false, fi.FileSize };
                                    db.ExecuteNonQuery(tran, "SYS_AddNeedUpdateFile", para).ToString();
                                }
                                if (this.progressBar1.Value < 98)
                                    this.progressBar1.Value += 1;
                                //FTP

                            }

                            BroadcastAutoId = Convert.ToInt32(db.ExecuteScalar(tran, "Broadcast_Edit", 0, newVsersion, "Upgrade", "", "ALL", "Admin", "N", "updates"));
                            tran.Commit();
                            this.lbl_vertify.Text = $"已經產生版本號的數據。公告號為: {BroadcastAutoId}";
                            //this.progressBar1.Value = 1;
                            //if (UploadFiles("192.168.88.53", "NMErpUpdate", "Nien123ErpUp", needUpdateFiles))
                            //{
                            //    this.lbl_vertify.Text = this.lbl_vertify.Text = $"已經更新，當前版本號為：{newVsersion}";
                            //    tran.Commit();
                            //}
                            //else
                            //{
                            //    tran.Rollback();
                            //}

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
            this.progressBar1.Value = 0;
            ///   stp = new System.Threading.Thread(new System.Threading.ThreadStart(UploadToServer));
            //  stp.Start();

        }

        //public void UploadToServer()
        //{

        //    FtpClient ftp = new FtpClient("192.168.88.53", "NMErpUpdate", "Nien123ErpUp");

        //    int step = 100;
        //    if (needUpdateFiles.Count > 0)
        //        step = 100 / needUpdateFiles.Count;
        //    if (step == 0) step = 1;
        //    foreach (ReleaseFileInfo fi in needUpdateFiles)
        //    {
        //        if (fi.FileName.IndexOf("defaultLoginer.xml") > -1) continue;

        //    //    this.lbl_vertify.Text = "正在上傳 " + fi.FilePath + " ...";
        //        System.Threading.Thread.Sleep(100);
        //        ftp.SubRootCount = 0;
        //        if (fi.FilePath.IndexOf("\\") > -1)
        //        {

        //            ftp.PureUploadSub(fi.TrueFilePath, fi.FilePath);
        //        }
        //        else
        //        {
        //            ftp.Upload(fi.TrueFilePath);
        //        }
        //        for (int i = 0; i < ftp.SubRootCount; i++)
        //        {
        //            ftp.ChangeDir("..");
        //        }
        //       // if (this.progressBar1.Value < 98)
        //        //   this.progressBar1.Value += step;
        //    }
        // //   progressBar1.Value = 100;
        // //   this.lbl_vertify.Text = "上傳完成。";
        //}


        System.Threading.Thread stp = null;
        private int CompairFileCount = 0;
        private async void button6_Click(object sender, EventArgs e)
        {

            //BroadcastAutoId = Convert.ToInt32(db.ExecuteScalar(tran, "Broadcast_Edit", 0, newVsersion, "Upgrade", "", "ALL", "Admin", "N", "updates"));


            return;

            this.lbl_vertify.Text = "正在驗證 ...";
            if (stp != null)
            {
                if (stp.IsAlive)
                    stp.Abort();
            }
            stp = new System.Threading.Thread(new System.Threading.ThreadStart(Compair));
            stp.Start();
        }
        public bool UploadFiles(string serverIp, string serverUserName, string serverPassword, List<ReleaseFileInfo> filselist)
        {
            string errfile = string.Empty;
            using (var ftp = new FtpClient(serverIp, serverUserName, serverPassword))
            {
                ftp.AutoConnect();
                foreach (var file in filselist)
                {
                    if (ftp.UploadFile(file.TrueFilePath, file.FilePath, FtpRemoteExists.Overwrite, true, FtpVerify.Retry) != FtpStatus.Success)
                    {
                        //
                        errfile = file.TrueFilePath;
                        break;
                    }
                }
                //ftp.Connect();

                // ftp.UploadFiles(filepaths, "", FtpRemoteExists.Overwrite, true, FtpVerify.Retry);
            }
            if (errfile.Length > 0)
            {
                this.lbl_vertify.Text = "有一些文件沒有上傳成功，請重新更新=>" + errfile;

                return false;

            }
            return true;
        }

        private void Compair()
        {
            this.lbl_vertify.Text = "暫時不需要驗證功能。";
            return;

            //     FtpClient ftp = new FtpClient("192.168.88.53", "NMErpUpdate", "Nien123ErpUp");
            //ftp.get
            string[] newFileList = System.IO.Directory.GetFiles(this.txt_basedif.Text, "*.*", System.IO.SearchOption.AllDirectories);
            List<ReleaseFileInfo> newFileData = new List<ReleaseFileInfo>();
            foreach (string f in newFileList)
            {
                if (f.IndexOf(".pdb") > -1) continue;
                if (f.IndexOf(".scc") > -1) continue;
                if (f.IndexOf(".db") > -1) continue;
                if (f.IndexOf("\\logs\\") > -1) continue;
                if (f.IndexOf("\\ref\\") > -1) continue;
                if (f.IndexOf("\\runtimes\\") > -1) continue;
                if (f.IndexOf("\\data\\UserSet\\") > -1) continue;
                if (f.IndexOf("Infragistics.") > -1 && f.IndexOf(".xml") > -1) continue;
                if (f.IndexOf(" defaultLoginer.xml") > -1) continue;
                if (f.StartsWith("ErpUpdate.")) continue;
                FileInfo fi = new FileInfo(f);
                ReleaseFileInfo file = new ReleaseFileInfo(f, f.Replace(this.txt_basedif.Text + "\\", ""), fi.Name, fi.LastWriteTime.Ticks, fi.Length);
                newFileData.Add(file);
            }
            this.progressBar1.Value = 0;
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
                    //serversize = ftp.GetFileSize(fi.FilePath);
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

                }
                if (this.progressBar1.Value < 98)
                    this.progressBar1.Value += step;

            }
            progressBar1.Value = 100;
            this.lbl_vertify.Text = "驗證完成。";
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stp != null)
            {
                if (stp.IsAlive) stp.Interrupt();
            }
            Application.DoEvents();
            Application.Exit();
        }



        private void deletelast_Click(object sender, EventArgs e)
        {
            SqlHelper db = DatabaseFactory.CreateDatabase();

            string sqlCommand = "";
            if (MessageBox.Show("確定要刪除嗎？", "Alert", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                sqlCommand = "declare @ver varchar(30);select @ver=Max(Version) from versions where isLive = 0;delete from versions where isLive = 0 and [Version]=@ver;if @@rowcount>0 delete AssemblyInfo where [Versions]=@ver;   ";
                db.ExecuteNonQuerySqlString(sqlCommand).ToString();
                MessageBox.Show("刪除成功");
            }
        }


        private static Orleans.IClusterClient _myclient = null;
        private static bool connectedSuccess = false;
        public static Orleans.IClusterClient _client
        {
            get
            {
                if (!connectedSuccess)
                {

                    if (_myclient != null)
                    {
                        if (_myclient.IsInitialized)
                        {
                            _myclient?.Close();
                            _myclient?.Dispose();
                        }
                    }
                    _myclient = GetClientBuilder().Build();
                    _myclient.Connect().Wait();
                    connectedSuccess = true;
                }
                return _myclient;
            }
        }
        public static Orleans.IClientBuilder GetClientBuilder()
        {

            Orleans.IClientBuilder _clientbuilder = new ClientBuilder()

                         .Configure<ClusterOptions>(options =>
                         {
                             options.ClusterId = AppConfig.ClusterId;
                             options.ServiceId = AppConfig.ServiceId;
                         })
                           .Configure<ClientMessagingOptions>(ops =>
                           {
                               ops.ResponseTimeout = TimeSpan.FromMinutes(1);

                               //   ops.NetworkInterfaceName
                           });

            //  .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(BusinessFacade.IBroadcast).Assembly).WithReferences());
            //   _clientbuilder.UseLocalhostClustering();
            _clientbuilder.UseRedisClustering(opt =>
            {
                opt.ConnectionString = AppConfig.HostServer;// "host:port";
                opt.Database = 3;
            });
            // _clientbuilder.UseLocalhostClustering();
            return _clientbuilder;

        }

        int BroadcastAutoIdLast = 0;
        private void button5_Click_1(object sender, EventArgs e)
        {

            if (BroadcastAutoId <= 0 || newVsersion.Length < 2)
            {
                MessageBox.Show("沒有產生新的版本數據。");
                return;
            }
            // BroadcastAutoId = 95;
            //  newVsersion = "1.02";
            //MessageBox.Show($"請到 微信“程序更新” 群，聯繫 運維同事 進行第3步的發佈！ 版本號為： \" {newVsersion.ToString()} \"    （兆豐：崔偉，任杰，億豐，泛昌：盛新民）");
            //return;


            if (BroadcastAutoId <= 0 || newVsersion.Length < 2)
            {
                MessageBox.Show("沒有產生新的版本數據。");
                return;
            }
            try
            {
                if (BroadcastAutoIdLast == BroadcastAutoId)
                {
                    MessageBox.Show($"已經發佈過此版本。{BroadcastAutoIdLast}");
                    return;
                }

                foreach (ReleaseFileInfo fi in needUpdateFiles)
                {
                    if (fi.isChanged)
                    {
                        string todir = fi.TrueFilePath.Replace(this.txt_basedif.Text, this.tbx_publishdir.Text).Replace("/", "\\");
                        todir = todir.Substring(0, todir.LastIndexOf("\\"));
                        if (!Directory.Exists(todir))
                        {
                            Directory.CreateDirectory(todir);
                        }
                        try
                        {
                            System.IO.File.Copy(fi.TrueFilePath, todir + "\\" + fi.FileName, true);
                        }
                        catch
                        {
                            if(RemoveReadOnly(todir + "\\" + fi.FileName))
                            {
                                try
                                {
                                    System.IO.File.Copy(fi.TrueFilePath, todir + "\\" + fi.FileName, true);
                                }catch(Exception ex1)
                                {
                                    throw ex1;
                                }
                            }
                        }
                    }
                }


                IVersions versionsBLL = _client.GetGrain<IVersions>(0);
                var ver = versionsBLL.GetModel(decimal.Parse(newVsersion)).Result;
                ver.OldLastActionCode = ver.LastActionCode;
                ver.OldLastActionTime = ver.LastActionTime;
                ver.OldLastActionUser = ver.LastActionUser;
                ver.LastActionCode = "A";
                ver.LastActionUser = "Admin";
                ver.isLive = true;
                ver = versionsBLL.Confirm(ver).Result;

                IBroadcast broadcastBLL = _client.GetGrain<IBroadcast>(0);
                BusinessEntity.BroadcastEntity model = broadcastBLL.GetModel(BroadcastAutoId).Result;
                model.OldLastActionCode = model.LastActionCode;
                model.OldLastActionTime = model.LastActionTime;
                model.OldLastActionUser = model.LastActionUser;
                model.LastActionCode = "A";
                model.LastActionUser = "Admin";
                model = broadcastBLL.Confirm(model).Result;
                this.lbl_vertify.Text = $"發佈成功。 公告號：{BroadcastAutoId}";
                BroadcastAutoIdLast = BroadcastAutoId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


            try
            {
                string[] newFileList = System.IO.Directory.GetFiles(this.txt_basedif.Text, "*.*", System.IO.SearchOption.AllDirectories);

                foreach (var fi in newFileList)
                {
                    try
                    {
                        System.IO.File.Delete(fi);
                    }
                    catch
                    {
                        if(RemoveReadOnly(Path.Combine(System.Environment.CurrentDirectory, fi)))
                        {

                            try
                            {
                                System.IO.File.Delete(fi);
                            }
                            catch
                            {

                            }
                        }
                     
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete files {this.txt_basedif.Text} error. {ex.Message}.");
                return;
            }
            //移除文件
        }

        private bool RemoveReadOnly(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                return true;
            }else
            {
                return false;
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            SqlHelper db = DatabaseFactory.CreateDatabase();

            string sqlCommand = "";
            if (MessageBox.Show("確定要刪除最新一個未發佈的版本嗎？", "Alert", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                sqlCommand = "declare @ver varchar(30);select @ver=Max(Version) from versions where isLive = 0;delete from versions where isLive = 0 and [Version]=isnull(@ver,0) and datediff(minute,lastactiontime,getdate())<15;if @@rowcount>0 begin delete AssemblyInfoList where [Version]=isnull(@ver,0); end else begin set @ver=0 end  select ver=isnull(@ver,0); ";
                 var ver= db.ExecuteScalarSqlString(sqlCommand);
                if(ver!= null) { 
                    if(decimal.TryParse(ver.ToString(),out decimal oldver))
                    {
                        if (oldver > 0)
                        {
                            MessageBox.Show("刪除成功:" + ver);
                            return;
                        }else
                        {
                            MessageBox.Show("發佈版本數據已經超過15分鐘，不能刪除。" );
                            return;
                        }
                    }
                }
                MessageBox.Show("沒有版本需要刪除。");

            }
        }
    }
    public class ReleaseFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string TrueFilePath { get; set; }
        public long FileDate { get; set; }
        public long FileSize { get; set; }

        public bool isChanged { get; set; }
        public ReleaseFileInfo(string fullpath, string fpath, string fname, long fdate, long fileSize)
        {
            FilePath = fpath;
            FileName = fname;
            FileDate = fdate;
            isChanged = true;
            TrueFilePath = fullpath;
            FileSize = fileSize;
        }
    }
}
