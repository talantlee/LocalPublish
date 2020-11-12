using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace LocalPublish
{
    
    /// <summary>
    /// TODO：下载更新目录是否完整的验证
    /// </summary>
    public partial class AutoUpdate : Form
    {
     
        System.Threading.Thread sftp = null;
        System.Collections.ArrayList downloadPath = new System.Collections.ArrayList();
        System.Collections.ArrayList VersionIds = new System.Collections.ArrayList();
        string ServerBaseUpdateDir = " HR\\Orders\\ABC";
        string serverIp = "127.0.0.1";
        string serverUserName = "ccc";
        string serverPassword = "ccc";
        int serverPort = 21;
        int timeoutsecondes = 900;
        /// <summary>
        /// 0-当前系统版本,1-最新版本,2-end 要更新的版本目录
        /// </summary>
        /// <param name="args"></param>
        public AutoUpdate(string[] argu):this()
        {
            if (argu != null)
            {
                if (argu.Length > 0)
                {
                    string[] args = argu[0].Split('&');
                   
                    if (args.Length <6)
                    {
                        return;
                       // 
                    }
                    this.lbl_currvesion.Text = args[0];
                    ServerBaseUpdateDir = args[7];
                    serverIp = args[3];
                    serverUserName = args[5];
                    serverPassword = args[6];
                    serverPort =Convert.ToInt32(args[4]);
                    //if (args.Length > 1)
                    //{
                    //    this.lbl_newVersion.Text = args[1];
                    //}
                    string[] versions = args[1].Split(',');
                    string[] versionids = args[2].Split(',');
                    for (int i = 0; i < versions.Length; i++)
                    {
                        if(versions[i].Length>0)
                        downloadPath.Add(versions[i]);
                    }
            
                    if(downloadPath.Count>0)
                    this.lbl_newVersion.Text = downloadPath[downloadPath.Count - 1].ToString();

                    for (int i = 0; i < versionids.Length; i++)
                    {
                        if (versionids[i].Length > 0)
                        VersionIds.Add(versionids[i]);
                    }

                    
                }
            }
        }
 
        public AutoUpdate()
        {
            InitializeComponent();
        }
        public void DownLoad()
        {
            if (downloadPath.Count > 0)
            {

                this.lbl_updateinfo.Text = "开始下载文件...";

                this.progressBar1.Value = 0;
                string basedesdir = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "updated";
             
                for (int i = 0; i < downloadPath.Count; i++)
                {
                    this.lbl_newVersion.Text ="正在下载版本"+ downloadPath[i].ToString();
                    ftpserver.DownloadDirectory(downloadPath[i].ToString(), basedesdir);
                }
          
                ftpserver.Close();
                progressBar1.Value = progressBar1.Maximum;
                this.lbl_updateinfo.Text = "下载完成.";

                System.Threading.Thread.Sleep(100);
                progressBar1.Value = 0;
                this.lbl_updateinfo.Text = "版本更新中...";
                //FileStream fs = File.Open(, FileMode.OpenOrCreate);
                //fs.SetLength(0);
                progressBar1.PerformStep();
              
                try
                {
                    for (int i = 0; i < downloadPath.Count; i++)
                    {
                        this.lbl_updateinfo.Text = "版本将升级到 " + downloadPath[i].ToString();

                        if (Directory.Exists(basedesdir + Path.DirectorySeparatorChar + downloadPath[i].ToString()))
                        {
                            if (Directory.GetFiles(basedesdir + Path.DirectorySeparatorChar + downloadPath[i].ToString(), "updatelist.txt").Length != 1)
                            {
                                this.lbl_updateinfo.Text = "更新失败,下载更新目录文件失败。";
                                return;
                            }
                            else
                            {
                                StreamReader sr = new StreamReader(basedesdir + Path.DirectorySeparatorChar + downloadPath[i].ToString() + Path.DirectorySeparatorChar + "updatelist.txt");
                                
                                string tmpfilename = sr.ReadLine();
                                while (tmpfilename!=null)
                                {
                                    if (!File.Exists(basedesdir + Path.DirectorySeparatorChar + downloadPath[i].ToString() + Path.DirectorySeparatorChar + tmpfilename))
                                    {
                                        this.lbl_updateinfo.Text = "更新[" + downloadPath[i].ToString() + "] 失败,文件:" + tmpfilename+"未找到";
                                        sr.Close();
                                        return;
                                    }
                                    tmpfilename = sr.ReadLine();
                                }
                                sr.Close();

                            }
                            string[] filelst = Directory.GetFiles(basedesdir + Path.DirectorySeparatorChar + downloadPath[i].ToString());

                            foreach (string s in filelst)
                            {
                                progressBar1.PerformStep();
                                string fileName = s.Substring(s.LastIndexOf("\\") + 1);
                                if (fileName.IndexOf(".zip", StringComparison.OrdinalIgnoreCase) > -1)
                                {
                                    //ICSharpCode.SharpZipLib.Zip.ZipHelper.UnZip(s, System.Environment.CurrentDirectory, false, string.Empty);

                                }
                                else
                                {

                                    File.Copy(s, Path.Combine(System.Environment.CurrentDirectory, fileName), true);
                                }
                            }
                            progressBar1.PerformStep();
                            Directory.Delete(basedesdir + Path.DirectorySeparatorChar + downloadPath[i].ToString(), true);
                            progressBar1.PerformStep();
                            this.lbl_newVersion.Text = downloadPath[i].ToString();
                            //     sw.WriteLine(downloadPath[i].ToString());
                            StreamWriter sw = new StreamWriter(System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "updated" + Path.DirectorySeparatorChar + "updatelog.txt", true);
                            sw.WriteLine(downloadPath[i].ToString() + "   " + VersionIds[i].ToString() + "    " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            this.lbl_currvesion.Text = downloadPath[i].ToString();
                            progressBar1.PerformStep();

                            //Update to Version
                            if (downloadPath.Count - 1 == i)
                            {
                                StreamWriter sw1 = new StreamWriter(System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "updatelog.log", false);

                                try
                                {
                                    sw1.WriteLine(VersionIds[i].ToString());
                                    sw1.Close();
                                }
                                finally
                                {
                                    sw1.Close();
                                }
                            }
                            while (progressBar1.Value < progressBar1.Maximum)
                                progressBar1.Value = progressBar1.Maximum;



                        }

                        else
                        {
                            MessageBox.Show("更新失败，请确认服务器的更新目录正确。Error:" + downloadPath[i].ToString());
                        }
                      
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("更新失败，请确认更新主程序已经关闭。Error:" + ex.ToString());
                }
                finally
                {
                  //  sw.Close();
                   // fs.Close();
                }
             
              
               

            }
            //TODO:更新
            this.lbl_updateinfo.Text = "更新完成.";
            btn_updated.Width = 120;
             this.btn_updated.Text = "启动程序";
            this.btn_updated.Visible = true;
            this.btn_updated.Click -= btn_updated_Click;

            btn_updated.Click += (obj, sd) =>
            {
                System.Diagnostics.Process.Start("WHERP-V3.exe");
                Application.DoEvents();
                Application.Exit();

            };
            if (sftp != null)
            {
                if (sftp.IsAlive)
                    sftp.Abort();
            }
          

        }

        //public void StarProgress()
        //{
        //    this.progressBar1.Visible = true;
        
        //    st = new System.Threading.Thread(new System.Threading.ThreadStart(AutoProgress));
        //    st.IsBackground = true;
        //    st.Start();
           

        //}

        //private void AutoProgress()
        //{
        //    while (true)
        //    {
        //        System.Threading.Thread.Sleep(25);
        //        progressBar1.PerformStep();
        //        if(isLoginSuccess)
        //        {
        //            while (progressBar1.Value < progressBar1.Maximum) progressBar1.PerformStep();
        //            progressBar1.Value = progressBar1.Maximum;
                  
        //            this.lbl_updateinfo.Text = "下载版本文件成功.";
                   
        //            sftp = new System.Threading.Thread(new ThreadStart(DownLoad));
        //            sftp.Start();
        //            //TODO:开始下载
        //            if (st != null)
        //            {
        //                if (st.IsAlive)
        //                    st.Abort();
        //            }
                    
                   
        //        }
              
        //    }
        //}
       
        FtpClient ftpserver = null;
     
        private void btn_updated_Click(object sender, EventArgs e)
        {
            this.btn_updated.Visible = false;
            if (downloadPath.Count > 0)
            {
                // StarProgress();
                ftpserver = new FtpClient(serverIp, serverUserName, serverPassword, timeoutsecondes, serverPort, this.progressBar1, this.lbl_updateinfo);
                ftpserver.RemotePath = ServerBaseUpdateDir;
                try
                {
                    ftpserver.Login();
                }
                catch
                {
                    this.lbl_updateinfo.Text = "服务器连接失败.";
                    return;
                }
                this.lbl_updateinfo.Text = "获取文件列表中...";
              
                sftp = new System.Threading.Thread(new ThreadStart(DownLoad));
                sftp.Start();
            }
            else
            {
                this.lbl_updateinfo.Text = "不需要更新.";
            }
        }

        private void AutoUpdate_Load(object sender, EventArgs e)
        {
            //if (this.lbl_currvesion.Text.Length == 0)
            //{
            //    if (File.Exists(System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "updatelog.log"))
            //    {
            //        StreamReader sr = new StreamReader(System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "updatelog.log");
            //       this.lbl_currvesion.Text= sr.ReadLine();
            //       sr.Close();
            //    }
            //}
        }
    }
}
