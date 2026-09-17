using AutoLocalPublish.Models;
using BusinessFacade;
using DataAccessLayers;
using Microsoft.Extensions.FileProviders;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
            this.lbl_user.Text = System.Environment.UserName;
        }
        public IList<string> currentUpdateFIles = new List<string>();
        public IList<string> currentUpdateFIlesBase = new List<string>();
        private void button5_Click(object sender, EventArgs e)
        {

            this.lbl_vertify.Text = $"正在驗證更新檔案...{AppConfig.PublishToDir}";
            currentUpdateFIles = new List<string>();
            currentUpdateFIlesBase = new List<string>();
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
            if (!Directory.Exists(Path.Combine(basedir, "RootExternalDLLs")))
            {
                Directory.CreateDirectory(Path.Combine(basedir, "RootExternalDLLs"));
                return;
            }
            //Move RootExternalDLLs
            string[] rootFileList = System.IO.Directory.GetFiles(basedir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            int sepcialbasedll = 0;
            if (AutoLocalPublish.Form1.RootExternalDLLs.Length > 0)
                foreach (string f in rootFileList)
                {
                    if (AutoLocalPublish.Form1.RootExternalDLLs.Contains(Path.GetFileName(f)) || extRootExternalDLLs.Contains(Path.GetFileName(f)))
                    {
                        bool dosuccess = false;
                        try
                        {

                            //if ("SharpDevelop.Base.dll" == Path.GetFileName(f) && f.IndexOf("RootExternalDLLs") > -1)
                            //{
                            //    sepcialbasedll++;
                            //}
                            //else if ("SharpDevelop.Base.dll" == Path.GetFileName(f) && f.IndexOf("RootExternalDLLs") == -1)
                            //{
                            //    sepcialbasedll--;
                            //}

                            if (File.Exists(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f))))
                            {

                                FileInfo file1 = new FileInfo(f);
                                FileInfo file2 = new FileInfo(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));

                                if (file1.LastWriteTime > file2.LastWriteTime)
                                {
                                    CopyFile(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                                }
                                else if ("SharpDevelop.Base.dll" != Path.GetFileName(f) && file1.LastWriteTime < file2.LastWriteTime)
                                {
                                    CopyFile(file2.FullName, file1.FullName);

                                }

                            }
                            else
                            {
                                CopyFile(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                            }

                            dosuccess = true;

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
                                            CopyFile(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                                        }
                                        dosuccess = true;
                                    }
                                    catch
                                    {
                                        MessageBox.Show($"無法移動RootExternalDLLs檔案{f}，請確認目錄是否有權限。");
                                        return;
                                    }
                                }
                        }
                        //delete original file
                        try
                        {
                            if (dosuccess && "SharpDevelop.Base.dll" != Path.GetFileName(f))
                                File.Delete(f);
                        }
                        catch
                        {
                            if (RemoveReadOnly(f))
                            {
                                try
                                {
                                    if (dosuccess && "SharpDevelop.Base.dll" != Path.GetFileName(f))
                                        File.Delete(f);
                                }
                                catch
                                {
                                    MessageBox.Show($"無法刪除原文件{f}。");
                                }

                            }

                        }
                    }
                }

            //if (sepcialbasedll > 0)
            //{
            //    try
            //    {
            //        FileInfo file1 = new FileInfo(Path.Combine(basedir, "SharpDevelop.Base.dll"));
            //        FileInfo file2 = new FileInfo(Path.Combine(basedir, "RootExternalDLLs", "SharpDevelop.Base.dll"));

            //        if (file1.LastWriteTime < file2.LastWriteTime)
            //        {
            //            ReplaceFile(file2.FullName, file1.FullName);
            //        }
            //    }
            //    catch
            //    {

            //    }
            //}
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
            if (basenewFileList?.Length == 0)
            {
                MessageBox.Show("更新目錄沒有文件。");
                return;
            }

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
            if (sb.Length > 0)
            {
                MessageBox.Show("根目錄只能存放 exe,dll,xml,.config,.json,.runtimeconfig,ico 這些文件，請確認文件目錄是否有放錯。" + sb.ToString());
                sb.Length = 0;
                return;
            }

            SqlHelper db1 = DatabaseFactory.CreateDatabase();
            OldData = db1.ExecuteDatasetSqlString("select * from AssemblyInfo order by fileDate desc;").Tables[0];
            //check files.
            string[] newFileList = System.IO.Directory.GetFiles(AppConfig.PublishToDir, "*.*", System.IO.SearchOption.AllDirectories);
            this.listView1.Items.Clear();
            List<ReleaseFileInfo> newFileData = new List<ReleaseFileInfo>();
         

            notAllowUpdateFiles = new List<string>();
            //todo:除了runtimes 或根目錄，其他地方不允許放置dll,exe.
           

            foreach (string f in newFileList)
            {
                if (f.IndexOf(".pdb") > -1) { continue; }
                if (f.IndexOf(".bat") > -1) { continue; }
                if (f.IndexOf(".scc") > -1) { continue; }
                if (f.ToLower().IndexOf(".lng") > -1) { continue; } // 不知道誰建立了一個 ChsEng.lng；
                if (f.IndexOf(".db") > -1) { continue; }
                if (f.IndexOf("\\ref\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }
                if (f.IndexOf("\\logs\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }
                if (f.IndexOf("\\RootExternalDLLs\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }
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
                }
                else
                {
                    ReleaseFileInfo file = new ReleaseFileInfo(f, f.Replace(AppConfig.PublishToDir + "\\", ""), fi.Name, fi.LastWriteTime.Ticks, fi.Length);
                    WriteLog($"Add:{file.FileName}=>{file.FileDate}");
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
                        if (dr["AssemblyPath"].ToString().Equals(fi.FilePath.Replace("/", "\\"), StringComparison.OrdinalIgnoreCase))
                        {

                            if (Convert.ToInt64(dr["FileDate"]) == fi.FileDate)
                            {
                                fi.isChanged = false;
                                // isFined = true;
                            }
                            break;
                        }
                    }
                    if (fi.isChanged)
                        needUpdateFiles.Add(fi);
                }
                // WriteLog($"正在比對檔案...需要更新的文件數:{needUpdateFiles.Count}");

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

                // WriteLog($"正在比對檔案...需要更新的文件數3:{needUpdateFiles.Count}");
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
                string userid = System.Environment.UserDomainName + "\\" + System.Environment.UserName;

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
                                newVsersion = db.ExecuteScalar(tran, "Versions_Edit", 0, "發佈器自動產生", userid, "N", "updates").ToString();
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
                                                MessageBox.Show("已清除上一次發佈異常殘留，請重新發佈一次。");
                                                return;
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
                                        currentUpdateFIles.Add(fi.FilePath);
                                        if (!fi.FilePath.Contains("\\"))
                                            if (!fi.FilePath.Contains("/"))
                                                currentUpdateFIlesBase.Add(fi.FileName);
                                        //  WriteLog($"By {System.Environment.UserName}  FileName={fi.FileName}");
                                    }
                                    if (this.progressBar1.Value < 98)
                                        this.progressBar1.Value += 1;
                                    //FTP

                                }

                                BroadcastAutoId = Convert.ToInt32(db.ExecuteScalar(tran, "Broadcast_Edit", 0, newVsersion, "Upgrade", "", "ALL", userid, "N", "updates"));
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

        public static string getServerLocation()
        {
            string Ipaddress = AppConfig.HostServer;

            if (Ipaddress.IndexOf("192.168.88.") > -1 || Ipaddress.IndexOf("192.168.89.") > -1 || Ipaddress.IndexOf("192.168.90.") > -1 || Ipaddress.IndexOf("192.168.91.") > -1 || Ipaddress.IndexOf("192.168.176.") > -1)
            {
                return "ZF";
            }
            else if (Ipaddress.IndexOf("192.168.4.") > -1 || Ipaddress.IndexOf("192.168.5.") > -1 || Ipaddress.IndexOf("192.168.6.") > -1 || Ipaddress.IndexOf("192.168.7.") > -1
            || Ipaddress.IndexOf("192.168.8.") > -1 || Ipaddress.IndexOf("192.168.9.") > -1 || Ipaddress.IndexOf("192.168.10.") > -1)
            {
                return "FC";
            }
            else if (Ipaddress.IndexOf("192.168.148.") > -1 || Ipaddress.IndexOf("192.168.149.") > -1 || Ipaddress.IndexOf("192.168.150.") > -1 || Ipaddress.IndexOf("192.168.151.") > -1)
            {
                return "YF";
            }
            else if (Ipaddress.IndexOf("192.168.112.") > -1 || Ipaddress.IndexOf("192.168.113.") > -1 || Ipaddress.IndexOf("192.168.114.") > -1 || Ipaddress.IndexOf("192.168.115.") > -1)
            {
                return "MX";
            }
            else if (Ipaddress.IndexOf("192.168.133.") > -1 || Ipaddress.IndexOf("192.168.134.") > -1 || Ipaddress.IndexOf("192.168.135.") > -1)
            {
                return "MX2";
            }
            else if (Ipaddress.IndexOf("192.168.188.") > -1) //|| Ipaddress.IndexOf("192.168.200.") > -1
            {
                return "US";
            }
            else if (Ipaddress.IndexOf("192.168.200.") > -1)
            {
                return "NIS";
            }
            else if (Ipaddress.IndexOf("192.168.160.") > -1 || Ipaddress.IndexOf("192.168.161.") > -1 || Ipaddress.IndexOf("192.168.162.") > -1 || Ipaddress.IndexOf("192.168.163.") > -1)
            {
                return "SF";
            }
            else if (Ipaddress.IndexOf("192.168.0.") > -1 || Ipaddress.IndexOf("192.168.1.") > -1 || Ipaddress.IndexOf("192.168.2.") > -1 || Ipaddress.IndexOf("192.168.3.") > -1)
            {
                return "TW";
            }
            else if (Ipaddress.IndexOf("192.168.168.") > -1 || Ipaddress.IndexOf("192.168.169.") > -1 || Ipaddress.IndexOf("192.168.170.") > -1 || Ipaddress.IndexOf("192.168.171.") > -1)
            {
                return "WL";
            }
            return "UnKnow";
        }
        private void WriteCurrentUpdateFilesToExcel()
        {
            string baseFileName = "NMERP-" + MaintenanceUpdate.getServerLocation() + ".xlsx";
            string userpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Environment.UserName);
            if (!Directory.Exists(userpath))
            {
                Directory.CreateDirectory(userpath);
            }
            string fileName = baseFileName;

            string filePath = Path.Combine(userpath, fileName);

            IWorkbook workbook = null;
            ISheet sheet = null;
            FileStream fs = null;
            bool fileExists = File.Exists(filePath);
            bool fileLocked = false;

            // 嘗試打開現有文件
            if (fileExists)
            {
                try
                {
                    fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    workbook = new XSSFWorkbook(fs);
                }
                catch (IOException)
                {
                    // 文件被佔用，創建新文件
                    fileLocked = true;
                    fileName = $"updatefiles{DateTime.Now:yyyyMMdd}.xlsx";
                    filePath = Path.Combine(userpath, fileName);
                    workbook = new XSSFWorkbook();
                }
            }
            else
            {
                workbook = new XSSFWorkbook();
            }

            // 取得或創建sheet
            sheet = workbook.NumberOfSheets > 0 ? workbook.GetSheetAt(0) : workbook.CreateSheet("UpdateFiles");

            // 找到最後一行
            int lastRowNum = sheet.LastRowNum;
            if (lastRowNum == 0 && sheet.GetRow(0) == null)
                lastRowNum = -1;

            // 如果是新文件，寫入標題
            if (lastRowNum == -1)
            {
                IRow header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Version");
                header.CreateCell(1).SetCellValue("User");
                header.CreateCell(2).SetCellValue("FileName");
                header.CreateCell(3).SetCellValue("UpdateDate");
            }

            // 追加數據
            int rowIndex = sheet.LastRowNum + 1;
            foreach (var file in currentUpdateFIlesBase)
            {
                IRow row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(newVsersion);
                row.CreateCell(1).SetCellValue(System.Environment.UserName);
                row.CreateCell(2).SetCellValue(file);
                row.CreateCell(3).SetCellValue(Form1.GetChinaTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }

            // 關閉舊文件流
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
            }

            // 保存
            using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(outFs);
            }
            // workbook.Close();
        }
        public void PublishToServer(IProgress<ScanProgress> progress = null)
        {
            if (BroadcastAutoId <= 0 || newVsersion.Length < 2)
            {
                MessageBox.Show("沒有產生新的版本數據。");
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = 100;

                return;
            }

            try
            {
                ProgressCount++;
               
                //todo 檢查是否需要備份
                if (CopyToBackUpServer() == false)
                {
                   
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = 100;
                    return;
                }
                string userid = System.Environment.UserDomainName + "\\" + System.Environment.UserName;

                IVersions versionsBLL = Form1._client.GetGrain<IVersions>(0);
                var ver = versionsBLL.GetModel(decimal.Parse(newVsersion)).Result;
                ver.OldLastActionCode = ver.LastActionCode;
                ver.OldLastActionTime = ver.LastActionTime;
                ver.OldLastActionUser = ver.LastActionUser;
                ver.LastActionCode = "A";
                ver.LastActionUser = userid;
                ver.isLive = true;
                ver = versionsBLL.Confirm(ver).Result;
                ProgressCount += 20;
                progress?.Report(new ScanProgress { CurrentCount = ProgressCount, CurrentFilePath = $"版本確認成功。 " });
                IBroadcast broadcastBLL = Form1._client.GetGrain<IBroadcast>(0);
                BusinessEntity.BroadcastEntity model = broadcastBLL.GetModel(BroadcastAutoId).Result;
                model.OldLastActionCode = model.LastActionCode;
                model.OldLastActionTime = model.LastActionTime;
                model.OldLastActionUser = model.LastActionUser;
                model.LastActionCode = "A";
                model.LastActionUser = userid;
                model = broadcastBLL.Confirm(model).Result;
                ProgressCount += 20;
                progress?.Report(new ScanProgress { CurrentCount = ProgressCount, CurrentFilePath = $"發佈成功。 公告號：{BroadcastAutoId}" });
                this.lbl_vertify.Text = $"發佈成功。 公告號：{BroadcastAutoId}";
                LogMessage($"發佈成功。 公告號：{BroadcastAutoId}");
                BroadcastAutoIdLast = BroadcastAutoId;
                WriteLog($"publish version.[{newVsersion}] By {System.Environment.UserName}  BroadcastAutoId={BroadcastAutoId}");

                if (currentUpdateFIles != null && currentUpdateFIles.Count > 0)
                {
                    foreach (var item in currentUpdateFIles)
                    {
                        ProgressCount ++;
                        progress?.Report(new ScanProgress { CurrentCount = ProgressCount, CurrentFilePath = $"Write Log{item}" });
                        WriteLog($"File={item}");
                    }
                }
                try
                {
                    if (currentUpdateFIlesBase != null && currentUpdateFIlesBase.Count > 0)
                    {
                   
                        WriteCurrentUpdateFilesToExcel();
                        ProgressCount += 20;
                        progress?.Report(new ScanProgress { CurrentCount = ProgressCount, CurrentFilePath = "Write Version Files." });
                    }
                }
                catch
                {

                }


                BroadcastAutoId = 0;

                needUpdateFiles = new List<ReleaseFileInfo>();
                this.listView1.Items.Clear();

                //Run Bat.File


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = 100;
            }
           
        }

        public bool CopyToBackUpServer()
        {
            if (!string.IsNullOrEmpty(AppConfig.CopyToBackUpServer))
            {
                if (System.IO.File.Exists(AppConfig.CopyToBackUpServer))
                {

                    var usererp = new System.Diagnostics.ProcessStartInfo(AppConfig.CopyToBackUpServer);
                    usererp.CreateNoWindow = true;
                    usererp.UseShellExecute = false;
                    var p = new Process();
                    p.StartInfo = usererp;
                    p.Start();
                    bool finishedInTime = p.WaitForExit(60000);
                    if (!finishedInTime)
                    {
                        // 3. 【关键】超时未完成的处理逻辑！
                        // 绝对不能直接显示“成功”，应该强制结束并报错
                        try
                        {
                            if (!p.HasExited) p.Kill(); // 强制杀死卡死的进程
                        }
                        catch { /* 忽略杀死进程时的异常 */ }

                        this.lbl_vertify.Text = $"發佈失敗：執行 {AppConfig.CopyToBackUpServer} 超時（超過60秒）！";
                        return false; // 终止后续逻辑
                    }
                    // 4. 【关键】检查外部程序的退出码（ExitCode）
                    // 约定：0 代表成功，非 0 代表失败
                    if (p.ExitCode == 0)
                    {
                        this.lbl_vertify.Text = $"發佈成功。 公告號：{BroadcastAutoId} ，執行完成。";
                        return true;
                    }
                    else
                    {
                        this.lbl_vertify.Text = $"發佈失敗：外部程序異常退出，錯誤：{AppConfig.CopyToBackUpServer}";
                        return false;
                    }

                }
            }
            return true;
        }

        public void WriteLog(string mess)
        {
            string userpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Environment.UserName);
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


        public DataTable OldData = new DataTable();
        static List<string> extRootExternalDLLs = new List<string>();
        static List<string> excludeFiles;
        static List<string> excludeBaseDir;
        static List<string> notAllowUpdateFiles;
        private void MaintenanceUpdate_Load(object sender, EventArgs e)
        {

            exfileAttrs = (List<FileAttr>)System.Configuration.ConfigurationManager.GetSection("FileConfig");
            extRootExternalDLLs = (List<string>)System.Configuration.ConfigurationManager.GetSection("ToRootList");


            excludeFiles = new List<string>();

             notAllowUpdateFiles = new List<string>();
            //todo:除了runtimes 或根目錄，其他地方不允許放置dll,exe.
            excludeBaseDir = new List<string>();
            exfileAttrs.ForEach(attr =>
            {
                if (attr.OpType.ToLower() == "exclude")
                {
                    excludeFiles.Add(attr.Key);
                }
                else if (attr.OpType.ToLower() == "allowsuddir")
                {
                    excludeBaseDir.Add(attr.Key);
                }
            });



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
        private void LogMessage(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => LogMessage(msg)));
                return;
            }
        
            this.lbl_status.Text = msg;
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            //if(!(System.Environment.UserName.IndexOf("ErpUpdate",StringComparison.OrdinalIgnoreCase) >-1 || System.Environment.UserName.IndexOf("dagger",StringComparison.OrdinalIgnoreCase) >-1))
            //{
            //    MessageBox.Show("你的 帳號不能測試。");
            //    return;
            //}
            currentUpdateFIles = new List<string>();
            currentUpdateFIlesBase = new List<string>();
            ProgressCount = 0;
            btnStartScan.Enabled = false;
            SqlHelper db1 = DatabaseFactory.CreateDatabase();
            OldData = db1.ExecuteDatasetSqlString("select * from AssemblyInfo order by fileDate desc;").Tables[0];
            progressBar1.Value = 0;

            notAllowUpdateFiles = new List<string>();
            //todo:除了runtimes 或根目錄，其他地方不允許放置dll,exe.
            IList<NeedCopyFile> needCopyFiles = new List<NeedCopyFile>();

            LogMessage($"準備掃描...{AppConfig.PublishToDir}");
          

            // 2. 定义进度回调（此 Lambda 表达式会自动在 UI 线程执行）
            var progress = new Progress<ScanProgress>(report =>
            {
                // 在这里更新 UI 控件，不会报跨线程异常
              //  lbl_status.Text = $"正在掃描：{report.CurrentFilePath}";
                LogMessage($"正在掃描：{report.CurrentFilePath}");
                // 如果知道总文件数，可以更新进度条百分比
                // progressBar1.Value = (int)((double)report.CurrentCount / totalCount * 100);

                // 如果不知道总数，可以设置为不确定模式 (Marquee)
                if (progressBar1.Style != ProgressBarStyle.Marquee)
                {
                    progressBar1.Style = ProgressBarStyle.Marquee;
                    progressBar1.MarqueeAnimationSpeed = 30;
                }
            });
            try
            {
                // 3. 传入 progress 对象进行异步扫描
                List<ReleaseFileInfo> snapshots = await GetFileSnapshotsAsync(AppConfig.PublishToDir, needCopyFiles,progress);

                LogMessage($"掃描{FileCount}完成！共發現 {snapshots.Count} 個文件 需要更新。");
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = 100;

                if (notAllowUpdateFiles.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var f in notAllowUpdateFiles) { sb.Append(f.ToString()).AppendLine(); }
             
                    MessageBox.Show("這些文件不允許更新到客戶端，請確認：\n" + sb.ToString());
                    return;
                }
                if (snapshots.Count == 0)
                {
                    this.lbl_vertify.Text = "沒有文件需要更新。";
                    LogMessage($"掃描{FileCount}完成！共發現 {snapshots.Count} 個文件 需要更新。");
                    return;
                }


                foreach (ReleaseFileInfo fi in snapshots)
                {
                    this.listView1.Items.Add(new ListViewItem(new string[] { fi.FilePath, fi.FileDate.ToString(), fi.isChanged ? "change" : "no change" }));
                }

            

                if (snapshots.Count > 0)
                {
                    BroadcastAutoId = 0;
                    if (MessageBox.Show("確定要發佈新版本嗎？", "Tips", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        this.progressBar1.Value = 0;
                        ProgressCount = 0;
                         var progress1 = new Progress<ScanProgress>(report =>
                        {
                            // 在这里更新 UI 控件，不会报跨线程异常
                            LogMessage($"版本文件存儲中到DataBase：{snapshots.Count}");
                            if (progressBar1.Style != ProgressBarStyle.Marquee)
                            {
                                progressBar1.Style = ProgressBarStyle.Marquee;
                                progressBar1.MarqueeAnimationSpeed = 30;
                            }
                        });
                       bool dosucess= SaveVerToDb(snapshots, needCopyFiles, progress1);

                        if (dosucess)
                        {
                            var progress2 = new Progress<ScanProgress>(report =>
                            {
                                // 在这里更新 UI 控件，不会报跨线程异常
                                LogMessage($"發佈版本到APP服務器，文件個數：{snapshots.Count}");
                                if (progressBar1.Style != ProgressBarStyle.Marquee)
                                {
                                    progressBar1.Style = ProgressBarStyle.Marquee;
                                    progressBar1.MarqueeAnimationSpeed = 30;
                                }
                            });
                            this.progressBar1.Value = 0;
                            ProgressCount = 0;
                            PublishToServer(progress2);
                        }

                    }
                }
               




            }
            catch (UnauthorizedAccessException ex)
            {
                LogMessage("掃描中斷：沒有權限訪問某些目錄。");
                MessageBox.Show($"掃描中斷：{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogMessage("掃描中斷：发生未知错误。");
                MessageBox.Show($"掃描中斷：{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 5. 恢复按钮状态
                btnStartScan.Enabled = true;
            }
            return;
           
         
        }
        public  bool SaveVerToDb(List<ReleaseFileInfo> snapshots, IList<NeedCopyFile> needCopys, IProgress<ScanProgress> progress = null)
        {
            SqlHelper db = DatabaseFactory.CreateDatabase();
            string userid = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
            newVsersion = string.Empty;

            //copy files
            using (IDbConnection connection = db.GetConnection())
            {
                connection.Open();
                IDbTransaction tran = connection.BeginTransaction();
                try
                {
                    newVsersion = db.ExecuteScalar(tran, "Versions_Edit", 0, "發佈器自動產生", userid, "N", "updates").ToString();
                    if (newVsersion == "-1")
                    {
                        //  
                        ProgressCount++;
                        string sqlCommand = "declare @ver varchar(30);select @ver=Max(Version) from versions where isLive = 0;delete from versions where isLive = 0 and [Version]=isnull(@ver,0) ;if @@rowcount>0 begin delete AssemblyInfoList where [Version]=isnull(@ver,0); end else begin set @ver=0 end  select ver=isnull(@ver,0); ";
                        var ver = db.ExecuteScalarSqlString(sqlCommand);
                        if (ver != null)
                        {
                            if (decimal.TryParse(ver.ToString(), out decimal oldver))
                            {
                                if (oldver > 0)
                                {
                                    WriteLog($"Already has one not publish version.[{ver}]");
                                    MessageBox.Show("已清除上一次發佈異常殘留，請重新發佈一次。");

                                    return false;
                                }
                                else
                                {
                                    MessageBox.Show("已經有一個版本未上線，請先上線上一個版本 或 刪除上一個未上線版本，請重新 點 “正式發佈”.");

                                    return false;
                                }
                            }
                        }

                    }
                    foreach (ReleaseFileInfo fi in snapshots)
                    {

                        if (fi.isChanged)
                        {
                            ProgressCount++;
                            object[] para = { newVsersion, fi.FileName, fi.FilePath, fi.FileDate, false, fi.FileSize, GetFileIntegrity(fi.TrueFilePath) };
                            db.ExecuteNonQuery(tran, "SYS_AddNeedUpdateFile", para).ToString();
                            currentUpdateFIles.Add(fi.FilePath);
                            if (!fi.FilePath.Contains("\\"))
                                if (!fi.FilePath.Contains("/"))
                                    currentUpdateFIlesBase.Add(fi.FileName);
                    
                            progress?.Report(new ScanProgress
                            {
                                CurrentCount = ProgressCount,
                                CurrentFilePath = fi.FilePath
                            });
                        }

                    }

                    BroadcastAutoId = Convert.ToInt32(db.ExecuteScalar(tran, "Broadcast_Edit", 0, newVsersion, "Upgrade", "", "ALL", userid, "N", "updates"));
                    tran.Commit();
                    LogMessage($"已經產生版本號的數據。公告號為: {BroadcastAutoId}");
                    this.lbl_vertify.Text = $"已經產生版本號的數據。公告號為: {BroadcastAutoId}";

                    /*Copy FIle*/
                    if(needCopys!=null&& needCopys.Count > 0)
                    {
                        if (!Directory.Exists(Path.Combine(AppConfig.PublishToDir, "RootExternalDLLs")))
                            Directory.CreateDirectory(Path.Combine(AppConfig.PublishToDir, "RootExternalDLLs"));
                    }
                    
                   
                    foreach (NeedCopyFile fi in needCopys)
                    {
                        LogMessage($"Copy File: {fi.FromPath} to {fi.ToPath}");
                        CopyFile(fi.FromPath, fi.ToPath);
                       // File.Copy(fi.FromPath, fi.ToPath, true);
                        ProgressCount++;
                        progress?.Report(new ScanProgress
                        {
                            CurrentCount = ProgressCount,
                            CurrentFilePath = fi.FromPath
                        });
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = 100;
                }

            }
        }
        public  bool CopyFile(string sourcePath, string targetPath)
        {
            // Ensure target directory exists
          //  Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? Path.GetDirectoryName(Environment.CurrentDirectory)!);

            // If target does not exist, just move
            if (!File.Exists(targetPath))
            {
                File.Copy(sourcePath, targetPath, true);
                return true;
            }

            try
            {
                File.Copy(sourcePath, targetPath, true);
                return true;
            }
            catch
            {
                try
                {
                    File.SetAttributes(targetPath, FileAttributes.Normal);
                    File.Copy(sourcePath, targetPath, true);
                    return true;
                }
                catch { }
                return false;
            }
            //}
        }
        private static int ProgressCount = 0;
        private  int FileCount = 0;
        /// <summary>
        /// 异步获取目录下的所有文件快照
        /// 注意：此方法会抛出异常（如权限不足），请在调用方进行 try-catch 处理
        /// </summary>
        public async Task<List<ReleaseFileInfo>> GetFileSnapshotsAsync(string rootDirectory, IList<NeedCopyFile> needCopyFiles ,IProgress<ScanProgress> progress = null)
        {
            // 使用 ConcurrentBag 保证线程安全，且并发添加性能优于 List
            var fileSnapshots = new ConcurrentBag<ReleaseFileInfo>();
            FileCount = 0;

            // 将耗时的 I/O 操作放入后台线程池执行，避免阻塞 WinForms UI 线程
            //await Task.Run(() =>
            //{
            //    // EnumerateFiles 延迟加载，比 GetFiles 更省内存且启动更快
            //    // 遇到无权限目录会直接抛出 UnauthorizedAccessException，满足中断需求
            //    foreach (var fullPath in Directory.EnumerateFiles(rootDirectory, "*", SearchOption.TopDirectoryOnly))
            //    {

            //        if (fullPath.EndsWith(".pdb",StringComparison.OrdinalIgnoreCase) ||
            //        fullPath.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
            //          fullPath.EndsWith(".scc", StringComparison.OrdinalIgnoreCase) ||
            //            fullPath.EndsWith(".lng", StringComparison.OrdinalIgnoreCase) ||
            //              fullPath.EndsWith(".db", StringComparison.OrdinalIgnoreCase) ||
            //                fullPath.EndsWith(".db", StringComparison.OrdinalIgnoreCase) ||
            //                fullPath.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) ||
            //                fullPath.EndsWith("defaultLoginer.xml", StringComparison.OrdinalIgnoreCase)||
            //                (fullPath.IndexOf("Infragistics.") > -1 && fullPath.IndexOf(".xml") > -1)
            //        ) { 
            //            continue;
            //        }

            //        //if (fullPath.ToLower().IndexOf(".lng") > -1) { continue; } // 不知道誰建立了一個 ChsEng.lng；
            //        //if (fullPath.IndexOf(".db") > -1) { continue; }
            //        if (fullPath.Contains("\\ref\\") || fullPath.Contains("\\logs\\")
            //             || fullPath.Contains("\\StartUp.exe.WebView2\\")
            //             || fullPath.Contains("\\NMERP.exe.WebView2\\")
            //             || fullPath.Contains("\\WebView2Data\\EBWebView\\")
            //             || fullPath.Contains("\\WebView2Data\\tempfiles\\")
            //             || fullPath.Contains("\\data\\UserSet\\")
            //             || fullPath.Contains("\\updated\\")
            //              || fullPath.Contains("\\RootExternalDLLs\\")
            //        ) continue;
            //        LogMessage($"比對 {fullPath} 中。");
            //        //if (fullPath.IndexOf("Infragistics.") > -1 && fullPath.IndexOf(".xml") > -1) { continue; }

            //        //if (fullPath.IndexOf("\\ref\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }
            //        //if (fullPath.IndexOf("\\logs\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }
            //        //if (fullPath.IndexOf("\\RootExternalDLLs\\", StringComparison.OrdinalIgnoreCase) > -1) { continue; }


            //        //if (fullPath.IndexOf("\\StartUp.exe.WebView2\\") > -1) { continue; }
            //        //if (fullPath.IndexOf("\\NMERP.exe.WebView2\\") > -1) { continue; }


            //        //    if (fullPath.IndexOf("\\WebView2Data\\EBWebView\\") > -1) { continue; }
            //        //    if (fullPath.IndexOf("\\WebView2Data\\tempfiles\\") > -1) { continue; }
            //        //    if (fullPath.IndexOf(".deps.json") > -1) { continue; }//dagger.li 2023-12-20



            //        //if (fullPath.IndexOf("\\data\\UserSet\\") > -1) { continue; }
            //        //if (fullPath.IndexOf("\\updated\\") > -1) { continue; }
            //        //if (fullPath.IndexOf("Infragistics.") > -1 && f.IndexOf(".xml") > -1) { continue; }
            //        //if (fullPath.IndexOf("defaultLoginer.xml") > -1) { continue; }
            //        string offsetpath = Path.GetFullPath(fullPath).Substring(Path.GetFullPath(rootDirectory).Length + 1);
            //        var fileInfo = new FileInfo(fullPath);
            //        long filedate=fileInfo.LastWriteTime.Ticks;
            //        long filesize = fileInfo.Length;

            //        if (fullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || fullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            //        {
            //            if (offsetpath == fileInfo.Name || offsetpath.StartsWith("runtimes", StringComparison.OrdinalIgnoreCase) || offsetpath.StartsWith("RootExternalDLLs", StringComparison.OrdinalIgnoreCase) || offsetpath.ToLower().IndexOf("printboxno")>-1)
            //            {

            //            }else
            //            {
            //                notAllowUpdateFiles.Add(offsetpath);
            //                continue;
            //            }
            //        }
            //        bool exclude = false;
            //        foreach (var item in excludeFiles)
            //        {
            //            if (offsetpath.IndexOf(item.ToLower(),StringComparison.OrdinalIgnoreCase) > -1)
            //            {
            //                notAllowUpdateFiles.Add(offsetpath);
            //                exclude = true;
            //                break;
            //            }
            //        }
            //        if (exclude) continue;
            //        if (AutoLocalPublish.Form1.RootExternalDLLs.Contains(Path.GetFileName(fullPath)) || extRootExternalDLLs.Contains(Path.GetFileName(fullPath)))
            //        {
            //            if (offsetpath == fileInfo.Name)//根目錄的dll  //|| offsetpath.Contains("RootExternalDLLs")
            //            {
            //                //check RootExternalDLLs
            //                if (File.Exists(Path.Combine(rootDirectory, "RootExternalDLLs", Path.GetFileName(fullPath))))
            //                {
            //                    FileInfo file2 = new FileInfo(Path.Combine(rootDirectory, "RootExternalDLLs", Path.GetFileName(fullPath)));
            //                    if (fileInfo.LastWriteTime > file2.LastWriteTime)
            //                    {

            //                        //record to copy
            //                        needCopyFiles.Add(new NeedCopyFile() { FromPath = fullPath, ToPath = Path.Combine(rootDirectory, "RootExternalDLLs", fileInfo.Name) });
            //                        //  File.Copy(fullPath, Path.Combine(rootDirectory, "RootExternalDLLs", Path.GetFileName(fullPath)), true);
            //                    }
            //                    else if (fileInfo.LastWriteTime < file2.LastWriteTime)
            //                    {
            //                        filedate = file2.LastWriteTime.Ticks;
            //                        filesize = file2.Length;
            //                        if ("SharpDevelop.Base.dll" == fileInfo.Name)
            //                        {
            //                            needCopyFiles.Add(new NeedCopyFile() { FromPath = file2.FullName, ToPath = fullPath });

            //                            // File.Copy(file2.FullName, fullPath, true);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        // File.Delete(fullPath);//時間相等
            //                        continue;
            //                    }
            //                }
            //            }
            //        }

            //        //比較文件大小
            //        bool isnotchange = false;
            //        foreach (DataRow dr in OldData.Rows)
            //        {
            //            if (dr["AssemblyPath"].ToString().Equals(offsetpath.Replace("/", "\\"), StringComparison.OrdinalIgnoreCase))
            //            {

            //                if (Convert.ToInt64(dr["FileDate"]) == filedate)
            //                {
            //                    isnotchange = true;
            //                    break;
            //                }
            //                break;
            //            }
            //        }
            //        if (!isnotchange)
            //        {
            //            var snapshot = new ReleaseFileInfo(fullPath, offsetpath, fileInfo.Name, filedate, filesize);
            //            fileSnapshots.Add(snapshot);
            //        }
            //        ProgressCount++;

            //         progress?.Report(new ScanProgress
            //        {
            //            CurrentCount = ProgressCount,
            //            CurrentFilePath = fullPath
            //        });
            //    }
            //});
            List<Task<bool>> taskList = new List<Task<bool>>();

           // await GetDirFileAsync(rootDirectory, fileSnapshots, needCopyFiles, progress, false);
            taskList.Add(GetDirFileAsync(rootDirectory, fileSnapshots, needCopyFiles, progress, false));
            foreach (var fulldir in Directory.EnumerateDirectories(rootDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                if (fulldir.EndsWith("RootExternalDLLs", StringComparison.OrdinalIgnoreCase) || fulldir.EndsWith("logs", StringComparison.OrdinalIgnoreCase) || fulldir.EndsWith("logs", StringComparison.OrdinalIgnoreCase))
                    continue;
                taskList.Add(GetDirFileAsync(fulldir, fileSnapshots, needCopyFiles, progress));
               //await  GetDirFileAsync(fulldir, fileSnapshots, needCopyFiles, progress);
            }
            bool[] results = await Task.WhenAll(taskList);
            // 转为 List 方便后续在内存中进行 LINQ 比对或排序
            return new List<ReleaseFileInfo>(fileSnapshots);
        }

        public async Task<bool> GetDirFileAsync(string rootDirectory, ConcurrentBag<ReleaseFileInfo> fileSnapshots,  IList<NeedCopyFile> needCopyFiles, IProgress<ScanProgress> progress = null,bool searchAllDirectories = true)
        {

            // 将耗时的 I/O 操作放入后台线程池执行，避免阻塞 WinForms UI 线程
            await Task.Run(() =>
            {
                // EnumerateFiles 延迟加载，比 GetFiles 更省内存且启动更快
                // 遇到无权限目录会直接抛出 UnauthorizedAccessException，满足中断需求
                foreach (var fullPath in Directory.EnumerateFiles(rootDirectory, "*", searchAllDirectories?SearchOption.AllDirectories: SearchOption.TopDirectoryOnly))
                {
                    FileCount++;

                    if (fullPath.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) ||
                    fullPath.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
                      fullPath.EndsWith(".scc", StringComparison.OrdinalIgnoreCase) ||
                        fullPath.EndsWith(".lng", StringComparison.OrdinalIgnoreCase) ||
                          fullPath.EndsWith(".db", StringComparison.OrdinalIgnoreCase) ||
                            fullPath.EndsWith(".db", StringComparison.OrdinalIgnoreCase) ||
                            fullPath.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) ||
                            fullPath.EndsWith("defaultLoginer.xml", StringComparison.OrdinalIgnoreCase) ||
                            (fullPath.IndexOf("Infragistics.") > -1 && fullPath.IndexOf(".xml") > -1)
                    )
                    {
                        continue;
                    }

                    //if (fullPath.ToLower().IndexOf(".lng") > -1) { continue; } // 不知道誰建立了一個 ChsEng.lng；
                    //if (fullPath.IndexOf(".db") > -1) { continue; }
                    if (fullPath.Contains("\\ref\\") || fullPath.Contains("\\logs\\")
                         || fullPath.Contains("\\StartUp.exe.WebView2\\")
                         || fullPath.Contains("\\NMERP.exe.WebView2\\")
                         || fullPath.Contains("\\WebView2Data\\EBWebView\\")
                         || fullPath.Contains("\\WebView2Data\\tempfiles\\")
                         || fullPath.Contains("\\data\\UserSet\\")
                         || fullPath.Contains("\\updated\\")
                          || fullPath.Contains("\\RootExternalDLLs\\")
                    ) continue;
                    LogMessage($"比對 {fullPath} 中。");
  
                    string offsetpath = Path.GetFullPath(fullPath).Substring(Path.GetFullPath(AppConfig.PublishToDir).Length + 1);
                    var fileInfo = new FileInfo(fullPath);
                    long filedate = fileInfo.LastWriteTime.Ticks;
                    long filesize = fileInfo.Length;

                    if (fullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || fullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        if (offsetpath == fileInfo.Name || offsetpath.StartsWith("runtimes", StringComparison.OrdinalIgnoreCase) || offsetpath.StartsWith("RootExternalDLLs", StringComparison.OrdinalIgnoreCase) || offsetpath.ToLower().IndexOf("printboxno") > -1)
                        {

                        }
                        else
                        {
                            notAllowUpdateFiles.Add(offsetpath);
                            continue;
                        }
                    }
                    bool exclude = false;
                    foreach (var item in excludeFiles)
                    {
                        if (offsetpath.IndexOf(item.ToLower(), StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            notAllowUpdateFiles.Add(offsetpath);
                            exclude = true;
                            break;
                        }
                    }
                    if (exclude) continue;
                    if (AutoLocalPublish.Form1.RootExternalDLLs.Contains(Path.GetFileName(fullPath)) || extRootExternalDLLs.Contains(Path.GetFileName(fullPath)))
                    {
                        if (offsetpath == fileInfo.Name)//根目錄的dll  //|| offsetpath.Contains("RootExternalDLLs")
                        {
                            //check RootExternalDLLs
                            if (File.Exists(Path.Combine(rootDirectory, "RootExternalDLLs", Path.GetFileName(fullPath))))
                            {
                                FileInfo file2 = new FileInfo(Path.Combine(rootDirectory, "RootExternalDLLs", Path.GetFileName(fullPath)));
                                if (fileInfo.LastWriteTime > file2.LastWriteTime)
                                {

                                    //record to copy
                                    needCopyFiles.Add(new NeedCopyFile() { FromPath = fullPath, ToPath = Path.Combine(rootDirectory, "RootExternalDLLs", fileInfo.Name) });
                                    //  File.Copy(fullPath, Path.Combine(rootDirectory, "RootExternalDLLs", Path.GetFileName(fullPath)), true);
                                }
                                else if (fileInfo.LastWriteTime < file2.LastWriteTime)
                                {
                                    filedate = file2.LastWriteTime.Ticks;
                                    filesize = file2.Length;
                                    if ("SharpDevelop.Base.dll" == fileInfo.Name)
                                    {
                                        needCopyFiles.Add(new NeedCopyFile() { FromPath = file2.FullName, ToPath = fullPath });

                                        // File.Copy(file2.FullName, fullPath, true);
                                    }
                                }
                                else
                                {
                                    // File.Delete(fullPath);//時間相等
                                    continue;
                                }
                            }
                        }
                    }

                    //比較文件大小
                    bool isnotchange = false;
                    foreach (DataRow dr in OldData.Rows)
                    {
                        if (dr["AssemblyPath"].ToString().Equals(offsetpath.Replace("/", "\\"), StringComparison.OrdinalIgnoreCase))
                        {

                            if (Convert.ToInt64(dr["FileDate"]) == filedate)
                            {
                                isnotchange = true;
                                break;
                            }
                            break;
                        }
                    }
                    if (!isnotchange)
                    {
                        var snapshot = new ReleaseFileInfo(fullPath, offsetpath, fileInfo.Name, filedate, filesize);
                        fileSnapshots.Add(snapshot);
                    }
                    ProgressCount++;

                    progress?.Report(new ScanProgress
                    {
                        CurrentCount = ProgressCount,
                        CurrentFilePath = fullPath
                    });
                }
            });
            return true;
        }
        public class NeedCopyFile
        {
            public string FromPath { get; set; }      // 当前已扫描的文件数
            public string ToPath { get; set; } // 当前正在扫描的文件路径
        }
        // 进度报告模型
        public class ScanProgress
        {
            public int CurrentCount { get; set; }      // 当前已扫描的文件数
            public string CurrentFilePath { get; set; } // 当前正在扫描的文件路径
        }
    }
}
