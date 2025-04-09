
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLocalPublish
{
    internal class AppConfig
    {




      //  public static string BinPath { get; set; }
        public static string HostServer { get; set; }
        public static string ClusterId { get; set; }
        public static string ServiceId { get; set; }

        public static string BackUpDir { get; set; }
        public static string PublishToDir { get; set; }

        public static string HashCompare { get; set; }
        



        //public static void InitConfig(IConfigurationRoot config) {


        //    BinPath =  config.GetSection("BinPath").Get<string>();

        //    HostServer = config.GetSection("HostServer").Get<string>();
        //    ClusterId = config.GetSection("ClusterId").Get<string>();
        //    ServiceId = config.GetSection("ServiceId").Get<string>();

        //}

    }
}
