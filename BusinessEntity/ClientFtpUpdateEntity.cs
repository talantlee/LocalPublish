using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessEntity
{
    
    public class ClientFtpUpdateEntity
    {
        public string CurrentVersion { get; set; }
        public string NewVersion { get; set; }
        public string NewVersionID { get; set; }
        public bool NeedUpdate { get; set; }

        public string FtpServer { get; set; }
        public string FtpUserID { get; set; }
        public string FtpUserPassword { get; set; }

        public string FtpPort { get; set; }

       public IList<ClientFtpUpdateFile> NeewUpdateFiles { get; set; }
    }

    public class ClientFtpUpdateFile
    {
        public string FileName { get; set;}
        public bool IsDeleted { get; set;}
    }
}
