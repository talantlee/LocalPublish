using DataAccessLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalPublish
{
    static class Program
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
            SqlHelper.DbConnectionStrings.Add("sysdata", "Data Source=192.168.88.240;Initial Catalog=SysData;user id=cn01;password=Cn01test;Connect Timeout=90;");
            SqlHelper.DbConnectionStrings.Add("default", "Data Source=192.168.88.240;Initial Catalog=SysData;user id=cn01;password=Cn01test;Connect Timeout=90;");
            Application.Run(new Form1());
        }
    }
}
