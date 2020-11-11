using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DataAccessLayers
{
    public class DatabaseFactory
    {
       
        public static IDictionary<string, SqlHelper> _instanceList = new Dictionary<string, SqlHelper>();



        static DatabaseFactory()
        {
            if (_instanceList == null)
            {
                _instanceList = new Dictionary<string, SqlHelper>();
            }
            if (!_instanceList.ContainsKey("default"))
            {
                SqlHelper newinstance = new SqlHelper("default");
                _instanceList.Add("default", newinstance);
            }
        }
        public static SqlHelper CreateDatabase(string databasename="Default")
        {
           
            if (!_instanceList.ContainsKey(databasename.ToLower()))
            {
                SqlHelper newinstance = new SqlHelper(databasename);
                _instanceList.Add(databasename.ToLower(),newinstance);
                return newinstance;
            } else
            {
                return _instanceList[databasename.ToLower()];
            }
   
        }
    }

    
}
