using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessEntity
{
    
    public class UserLoginEntity: SimpleUserLoginEntity
    {

        public string ClientVersion { get; set; }

        public string UserName
        {
            get; set;
        }
      
       
        /// 用户登入时间
        public DateTime LoginTime
        {
            get; set;
        }


        /// 登出时间
        public DateTime LogoutTime
        {
           get;set;
        }

        public string ComID { get; set; }

        public string DeptNo { get; set; }
    }


    public class SimpleUserLoginEntity
    {
        public Guid LoginCode
        {
            get; set;
        }

        /// 用户ID
        public string UserCode
        {
            get; set;
        }

        /// 用户登入用的电脑名
        public string ComputerName
        {
            get; set;
        }

        /// 用户登入用的电脑IP
        public string ComputerIP
        {
            get; set;
        }

    }
}
