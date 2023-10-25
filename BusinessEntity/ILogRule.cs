
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessEntity
{
    public enum LOGTYPE { MASTER=0, DETAIL=1, STOREPROCEDURE=2 };

    public interface ILogRule
    {
        /// <summary>
        /// 对应ObjectAttribute表中的ObjectCode
        /// </summary>
        //string ObjectCode
        //{
        //    //set;
        //    get;
        //}
        /// <summary>
        /// 对应UI 中的ObjectCode
        /// </summary>
        string UIObjectCode
        {
            //set;
            get;
        }
        /// <summary>
        /// 对应ObjectAttribute表中的WitchTable
        /// </summary>
        string TableName
        {
            get;
        }

        /// <summary>
        /// 对象实例标识
        /// </summary>
        string ObjectInstanceKey
        {
            //set;
            get;
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        LOGTYPE LogType
        {
            //set;
            get;
        }
        /// <summary>
        /// 日志Sql字符种
        /// </summary>
        string LogSqlStr
        {
            set;
            get;
        }

        /// <summary>
        /// 日志描述
        /// </summary>
        string LogDescription
        {
            set;
            get;

        }

        /// <summary>
        /// 日志动作
        /// </summary>
        string LastActionCode
        {

            //set;
            get;
        }
        /// <summary>
        /// 日志时间
        /// </summary>
        DateTime LastActionTime
        {
            set;
            get;
        }
        /// <summary>
        /// 日志用户(加入日志的人)
        /// </summary>
        string LastActionUser
        {
            //set;
            get;
        }
        /*
         declare @sql varchar(3000)
        set @sql=''
        select @sql=@sql+'case "'+lower(name)+'":  return '+name+';'+CHAR(13)
            from sys.columns where object_id=object_id('PO_RpoMaster')
        print @sql
         */
       
         object this[string key]
        {
            get;
        }
        
        //Redis;
     

    }
}

