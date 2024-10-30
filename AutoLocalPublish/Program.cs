using DataAccessLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoLocalPublish
{
    internal static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SqlHelper.DbConnectionStrings = new Dictionary<string, string>();
            //    SqlHelper.DbConnectionStrings.Add("sysdata", "Data Source=192.168.88.53;Initial Catalog=SysData;user id=mis002;password=dongguan;Connect Timeout=90;");
            for (int i = 0; i < System.Configuration.ConfigurationManager.ConnectionStrings.Count; i++)
            {
                SqlHelper.DbConnectionStrings.Add(System.Configuration.ConfigurationManager.ConnectionStrings[i].Name, System.Configuration.ConfigurationManager.ConnectionStrings[i].ConnectionString);
            }

            // AppConfig.BinPath = System.Configuration.ConfigurationManager.AppSettings.Get("BinPath");
            AppConfig.HostServer = System.Configuration.ConfigurationManager.AppSettings.Get("HostServer");
            AppConfig.ServiceId = System.Configuration.ConfigurationManager.AppSettings.Get("ServiceId");
            AppConfig.ClusterId = System.Configuration.ConfigurationManager.AppSettings.Get("ClusterId");
            AppConfig.BackUpDir = System.Configuration.ConfigurationManager.AppSettings.Get("BackUpDir");
            AppConfig.PublishToDir = System.Configuration.ConfigurationManager.AppSettings.Get("PublishToDir");
            
            Application.Run(new MaintenanceUpdate());
        }
    }
}
