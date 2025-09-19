using AutoLocalPublish.Models;
using BusinessFacade;
using DataAccessLayers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace AutoLocalPublish
{
    public partial class MaintenanceUpdate : Form
    {
        static List<FileAttr> exfileAttrs = new List<FileAttr>();
        string backupdir = string.Empty;
        string currentVersion = string.Empty;
        List<ReleaseFileInfo> needUpdateFiles = new List<ReleaseFileInfo>();
        bool isbackupsuccess = false;
        int BroadcastAutoId = 0;
        string newVsersion = string.Empty;
      
        int BroadcastAutoIdLast = 0;
        public MaintenanceUpdate()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string  sqlCommand = "select * from AssemblyInfo order by fileDate desc;";
              SqlHelper db = DatabaseFactory.CreateDatabase();
            OldData = db.ExecuteDatasetSqlString(sqlCommand).Tables[0];
            this.lbl_vertify.Text = $"正在驗證更新檔案...{AppConfig.PublishToDir}";
            Publish();

          


        }
        public static string GetFileIntegrity(string filePath)
        {
            if (AppConfig.HashCompare != "1")
            {
                return string.Empty;
            }
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hashBytes = md5.ComputeHash(stream);
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetFileIntegrity({filePath}):" + ex.Message);
                return string.Empty;

            }
        }
        private bool RemoveReadOnly(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void moveRootDlls(string basedir)
        {
            //Move RootExternalDLLs
            string[] rootFileList = System.IO.Directory.GetFiles(basedir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            if (AutoLocalPublish.Form1.RootExternalDLLs.Length > 0)
                foreach (string f in rootFileList)
                {
                    if (AutoLocalPublish.Form1.RootExternalDLLs.Contains(Path.GetFileName(f)))
                    {
                        try
                        {
                            if (!Directory.Exists(Path.Combine(basedir, "RootExternalDLLs")))
                            {
                                Directory.CreateDirectory(Path.Combine(basedir, "RootExternalDLLs"));
                            }
                            if(File.Exists(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)))){

                                FileInfo file1 = new FileInfo(f);
                                FileInfo file2 = new FileInfo(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));

                                if(file1.LastWriteTime > file2.LastWriteTime)
                                {
                                    File.Copy(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                                }
                               
                            }
                            else
                            {
                                File.Copy(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                            }
                           
                        }
                        catch
                        {
                            if (File.Exists(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f))))
                                if (RemoveReadOnly(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f))))
                                {
                                    try
                                    {
                                        FileInfo file1 = new FileInfo(f);
                                        FileInfo file2 = new FileInfo(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));

                                        if (file1.LastWriteTime > file2.LastWriteTime)
                                        {
                                            File.Copy(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                                        }
                                    }
                                    catch
                                    {
                                        MessageBox.Show("無法移動RootExternalDLLs檔案，請確認目錄是否有權限。");
                                        return;
                                    }
                                }
                        }

                    }
                }
        }
        private static bool isRootExtentDll(string filename)
        {
            return AutoLocalPublish.Form1.RootExternalDLLs.Contains(filename);
        }
        private void Publish()
        {
            BroadcastAutoId = 0;
        
            needUpdateFiles = new List<ReleaseFileInfo>();
            if (this.tbx_publishdir.Text.Length == 0)
            {
                MessageBox.Show("未設置更新目錄。");
                return;
            }
         
            moveRootDlls(AppConfig.PublishToDir);
            //TODO:1 根目錄 只能放 exe,dll,xml,.config,.json,.runtimeconfig,ico
            string[] basenewFileList = System.IO.Directory.GetFiles(AppConfig.PublishToDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
          
            StringBuilder sb = new StringBuilder();
            foreach (string f in basenewFileList)
            {
                if (f.ToLower().IndexOf(".exe") > -1
                    || f.ToLower().IndexOf(".dll") > -1
                       || f.ToLower().IndexOf(".config") > -1
                        || f.ToLower().IndexOf(".xml") > -1
                          || f.ToLower().IndexOf(".json") > -1
                            || f.ToLower().IndexOf(".pdb") > -1
                       || f.ToLower().IndexOf(".lng") > -1 // 不知道誰建立了一個 ChsEng.lng；
                    || f.ToLower().IndexOf(".html") > -1 //RectangleLayout.html 
                             || f.ToLower().IndexOf(".bat") > -1
                           || f.ToLower().IndexOf(".runtimeconfig") > -1
                            || f.ToLower().IndexOf(".ico") > -1
                             || f.ToLower().IndexOf(".db") > -1
                    )
                {
                    continue;
                }
                else
                {
                    sb.Append(f).AppendLine(); 
                  
                 
                }
            }
            if(sb.Length> 0) {
                MessageBox.Show("根目錄只能存放 exe,dll,xml,.config,.json,.runtimeconfig,ico 這些文件，請確認文件目錄是否有放錯。"+sb.ToString());
                sb.Length = 0;
                return;
            }

            //check files.
            string[] newFileList = System.IO.Directory.GetFiles(AppConfig.PublishToDir, "*.*", System.IO.SearchOption.AllDirectories);
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
          
            foreach (string f in newFileList)
            {
                if (f.IndexOf(".pdb") > -1) { continue; }
                if (f.IndexOf(".bat") > -1) { continue; }
                if (f.IndexOf(".scc") > -1) { continue; }
                if (f.ToLower().IndexOf(".lng") > -1) { continue; } // 不知道誰建立了一個 ChsEng.lng；
                if (f.IndexOf(".db") > -1) { continue; }
                if (f.IndexOf("\\ref\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }
                if (f.IndexOf("\\logs\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }
                if (f.IndexOf("\\RootExternalDLLs\\",StringComparison.OrdinalIgnoreCase) > -1) { continue; }
                //   if (f.IndexOf("\\runtimes\\") > -1) {

                //win-x64,win-x86,win-arm64,
                //    continue;
                //   }


                if (f.IndexOf("\\StartUp.exe.WebView2\\") > -1) { continue; }
                if (f.IndexOf("\\NMERP.exe.WebView2\\") > -1) { continue; }


                if (f.IndexOf("\\WebView2Data\\EBWebView\\") > -1) { continue; }
                if (f.IndexOf("\\WebView2Data\\tempfiles\\") > -1) { continue; }
                if (f.IndexOf(".deps.json") > -1) { continue; }//dagger.li 2023-12-20



                if (f.IndexOf("\\data\\UserSet\\") > -1) { continue; }
                if (f.IndexOf("\\updated\\") > -1) { continue; }
                if (f.IndexOf("Infragistics.") > -1 && f.IndexOf(".xml") > -1) { continue; }
                if (f.IndexOf("defaultLoginer.xml") > -1) { continue; }

                FileInfo fi = new FileInfo(f);
                //Special Dir
                if (f.IndexOf("\\RootExternalDLLs\\", StringComparison.OrdinalIgnoreCase) > -1) 
                {
                    //判斷是否已經存在，如果存在，則比較時間，如果時間比較新，則覆蓋。
                    bool isFindInBase = false;
                    foreach (var item in newFileData)
                    {
                        if (item.FilePath.Equals(f.Replace(AppConfig.PublishToDir + "\\", "").Replace("RootExternalDLLs\\", "")))
                        {
                            if (fi.LastWriteTime.Ticks > item.FileDate)
                            {
                                item.FileDate = fi.LastWriteTime.Ticks;
                            }
                            isFindInBase = true;
                            break;
                        }
                    }
                    if (!isFindInBase)
                    {
                        ReleaseFileInfo file = new ReleaseFileInfo(f, f.Replace(AppConfig.PublishToDir + "\\", "").Replace("RootExternalDLLs\\", ""), fi.Name, fi.LastWriteTime.Ticks, fi.Length);
                        newFileData.Add(file);
                    }
                }else
                {
                    ReleaseFileInfo file = new ReleaseFileInfo(f, f.Replace(AppConfig.PublishToDir + "\\", ""), fi.Name, fi.LastWriteTime.Ticks, fi.Length);
                    newFileData.Add(file);
                }
                //  if (f.StartsWith("ErpUpdate.")) continue;

               

            }
            //RootExternalDLLs 處理
            string[] rootFileList = System.IO.Directory.GetFiles(Path.Combine(AppConfig.PublishToDir, "RootExternalDLLs"), "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (string f in rootFileList)
            {
                FileInfo fi = new FileInfo(f);
                bool isFindInBase = false;
                foreach (var item in newFileData)
                {
                    if (item.FilePath.Equals(f.Replace(AppConfig.PublishToDir + "\\", "").Replace("RootExternalDLLs\\", "")))
                    {
                        if (fi.LastWriteTime.Ticks > item.FileDate)
                        {
                            item.FileDate = fi.LastWriteTime.Ticks;
                        }
                        isFindInBase = true;
                        break;
                    }
                }
                if (!isFindInBase)
                {
                    ReleaseFileInfo file = new ReleaseFileInfo(f, f.Replace(AppConfig.PublishToDir + "\\", "").Replace("RootExternalDLLs\\", ""), fi.Name, fi.LastWriteTime.Ticks, fi.Length);
                    newFileData.Add(file);
                }
            }


            if (newFileData.Count > 0)
            {
               
                foreach (ReleaseFileInfo fi in newFileData)
                {

                    foreach (DataRow dr in OldData.Rows)
                    {
                        if (dr["AssemblyPath"].ToString().Equals(fi.FilePath.Replace("/","\\"), StringComparison.OrdinalIgnoreCase))
                        {

                            if (Convert.ToInt64(dr["FileDate"]) == fi.FileDate)
                            {
                                fi.isChanged = false;
                                // isFined = true;
                            }
                            break;
                        }
                    }
                    if(fi.isChanged)
                    needUpdateFiles.Add(fi);

                }
                foreach (ReleaseFileInfo fi in needUpdateFiles)
                {
                    bool isexclude = false;
                    foreach (var item in excludeFiles)
                    {
                        if (fi.FilePath.ToLower().IndexOf(item.ToLower()) > -1)
                        {
                            isexclude = true;
                            break;
                        }
                    }
                    if (!isexclude)
                    {
                        if (fi.FilePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || fi.FilePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            // //todo:除了runtimes 或根目錄，其他地方不允許放置dll,exe.
                            var subbasedir = fi.FilePath.Replace("/", "\\").Replace(AppConfig.PublishToDir.Replace("/", "\\") + "\\", "");

                            if (subbasedir.Contains("\\") && !subbasedir.StartsWith("runtimes", StringComparison.OrdinalIgnoreCase) && !subbasedir.StartsWith("RootExternalDLLs", StringComparison.OrdinalIgnoreCase) && subbasedir.ToLower().IndexOf("printboxno") == -1)
                            {
                                isexclude = true;
                                foreach (var item in excludeBaseDir)
                                {
                                    if (fi.FilePath.ToLower().IndexOf(item.ToLower()) > -1)
                                    {
                                        isexclude = false;
                                        break;
                                    }
                                }
                            }

                        }
                    }

                    if (isexclude)
                    {
                        notAllowUpdateFiles.Add(fi.FilePath); continue;
                    }
                    this.listView1.Items.Add(new ListViewItem(new string[] { fi.FilePath, fi.FileDate.ToString(), fi.isChanged ? "change" : "no change" }));
                }
                if (!needUpdateFiles.ToList<ReleaseFileInfo>().Any(att => att.isChanged))
                //  if (needUpdateFiles.Count == 0)
                {
                    this.lbl_vertify.Text = "沒有文件需要更新。";
                    return;
                }
                //TODO: 禁止一些文件更新到客戶端。
                if (notAllowUpdateFiles.Count > 0)
                {
                    sb = new StringBuilder();
                    foreach (var f in notAllowUpdateFiles) { sb.Append(f.ToString()).AppendLine(); }
                    MessageBox.Show("這些文件不允許更新到客戶端，請確認：\n" + sb.ToString());
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
                                    //  

                                   string sqlCommand = "declare @ver varchar(30);select @ver=Max(Version) from versions where isLive = 0;delete from versions where isLive = 0 and [Version]=isnull(@ver,0) ;if @@rowcount>0 begin delete AssemblyInfoList where [Version]=isnull(@ver,0); end else begin set @ver=0 end  select ver=isnull(@ver,0); ";
                                    var ver = db.ExecuteScalarSqlString(sqlCommand);
                                    if (ver != null)
                                    {
                                        if (decimal.TryParse(ver.ToString(), out decimal oldver))
                                        {
                                            if (oldver > 0)
                                            {
                                                WriteLog($"Already has one not publish version.[{ver}]");
                                            }
                                            else
                                            {
                                                MessageBox.Show("已經有一個版本未上線，請先上線上一個版本 或 刪除上一個未上線版本，請重新 點 “正式發佈”.");
                                             
                                                return;
                                            }
                                        }
                                    }
                                   
                                }

                                foreach (ReleaseFileInfo fi in needUpdateFiles)
                                {
                                    if (fi.isChanged)
                                    {
                                        object[] para = { newVsersion, fi.FileName, fi.FilePath, fi.FileDate, false, fi.FileSize, GetFileIntegrity(fi.TrueFilePath) };
                                        db.ExecuteNonQuery(tran, "SYS_AddNeedUpdateFile", para).ToString();
                                    }
                                    if (this.progressBar1.Value < 98)
                                        this.progressBar1.Value += 1;
                                    //FTP

                                }

                                BroadcastAutoId = Convert.ToInt32(db.ExecuteScalar(tran, "Broadcast_Edit", 0, newVsersion, "Upgrade", "", "ALL", "Admin", "N", "updates"));
                                tran.Commit();
                                this.lbl_vertify.Text = $"已經產生版本號的數據。公告號為: {BroadcastAutoId}";
                            
                                this.progressBar1.Value = 100;
                            }
                            catch (Exception ex)
                            {
                                tran.Rollback();
                                throw ex;
                            }

                        }
                     
                    }
                }
            

            }


            PublishToServer();

            return;


        }

        public void PublishToServer()
        {
            if (BroadcastAutoId <= 0 || newVsersion.Length < 2)
            {
                MessageBox.Show("沒有產生新的版本數據。");
                return;
            }
    
            try
            {
              

                IVersions versionsBLL = Form1._client.GetGrain<IVersions>(0);
                var ver = versionsBLL.GetModel(decimal.Parse(newVsersion)).Result;
                ver.OldLastActionCode = ver.LastActionCode;
                ver.OldLastActionTime = ver.LastActionTime;
                ver.OldLastActionUser = ver.LastActionUser;
                ver.LastActionCode = "A";
                ver.LastActionUser = System.Environment.UserName;
                ver.isLive = true;
                ver = versionsBLL.Confirm(ver).Result;

                IBroadcast broadcastBLL = Form1._client.GetGrain<IBroadcast>(0);
                BusinessEntity.BroadcastEntity model = broadcastBLL.GetModel(BroadcastAutoId).Result;
                model.OldLastActionCode = model.LastActionCode;
                model.OldLastActionTime = model.LastActionTime;
                model.OldLastActionUser = model.LastActionUser;
                model.LastActionCode = "A";
                model.LastActionUser = System.Environment.UserName;
                model = broadcastBLL.Confirm(model).Result;
                this.lbl_vertify.Text = $"發佈成功。 公告號：{BroadcastAutoId}";
                BroadcastAutoIdLast = BroadcastAutoId;
                WriteLog($"publish version.[{newVsersion}] By {System.Environment.UserName}  BroadcastAutoId={BroadcastAutoId}");
                BroadcastAutoId = 0;

                needUpdateFiles = new List<ReleaseFileInfo>();
                this.listView1.Items.Clear();

                //Run Bat.File
                if (!string.IsNullOrEmpty(AppConfig.CopyToBackUpServer))
                {
                    if (System.IO.File.Exists(AppConfig.CopyToBackUpServer))
                    {

                        var usererp = new System.Diagnostics.ProcessStartInfo(AppConfig.CopyToBackUpServer);
                        usererp.CreateNoWindow = true;
                        var p = new Process();
                        p.StartInfo = usererp;
                        p.Start();
                        int rollcheck = 8;
                        while (rollcheck > 0)
                        {
                            if (p.WaitForExit(2000))
                            {
                                break;
                            }
                            rollcheck--;
                        }
                        this.lbl_vertify.Text = $"發佈成功。 公告號：{BroadcastAutoId} ，執行{AppConfig.CopyToBackUpServer}完成。";
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public void WriteLog(string mess)
        {
            string userpath = System.IO.Path.Combine(System.Environment.CurrentDirectory, System.Environment.UserName);
            if (!Directory.Exists(userpath))
            {
                Directory.CreateDirectory(userpath);
            }
         
            using (StreamWriter sw = new StreamWriter(Path.Combine(userpath, "UpdateTips.txt"), true, Encoding.UTF8))
            {

                sw.WriteLine(System.DateTime.UtcNow + "=>" + mess);
                sw.Flush();
                sw.Close();
            }
        }


        public DataTable OldData=new DataTable();
        private void MaintenanceUpdate_Load(object sender, EventArgs e)
        {

            exfileAttrs = (List<FileAttr>)System.Configuration.ConfigurationManager.GetSection("FileConfig");


            SqlHelper db = DatabaseFactory.CreateDatabase();

            string sqlCommand = "";

            sqlCommand = "select Max(Version) from versions where isLive = 1 ";



            currentVersion = db.ExecuteScalarSqlString(sqlCommand).ToString();


            if (AppConfig.PublishToDir.Length > 0)
            {
                if (AppConfig.PublishToDir.EndsWith("\\"))
                {
                    this.tbx_publishdir.Text = AppConfig.PublishToDir;
                }
                else
                {
                    this.tbx_publishdir.Text = AppConfig.PublishToDir + "\\";
                }
            }
            this.lbl_vertify.Text = $"當前版本號為：{currentVersion}";
       
        }
    }
}
