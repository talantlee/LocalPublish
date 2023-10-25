
/*
* $Author:
* $Date:2021/2/20 9:45:47
* $Revision:
*/
using System;
using System.Collections.Generic;
using System.Text;
using BusinessEntity;
namespace BusinessEntity
{
    ///// <summary>
    ///// 
    ///// </summary>
    
    public class BroadcastEntity: BroadcastMessage,ILogRule
    {
      
        public BroadcastEntity()
        {
        }
        #region public field
      
        //public int AutoId { get; set; }

        //public MsgType MessageType { get; set; }

        public void SetMessageType(string messagetype)
        {
            switch (messagetype.ToLower())
            {
                case "broadcast":
                    MessageType = MsgType.BroadCast;
                    break;
                case "upgrade":
                    MessageType = MsgType.Upgrade;
                    break;
                case "command":
                    MessageType = MsgType.Command;
                    break;
                case "lock":
                    MessageType = MsgType.Lock;
                    break;
                case "clearclientcache":
                    MessageType = MsgType.ClearClientCache;
                    break;
                case "clearservercache":
                    MessageType = MsgType.ClearServerCache;
                    break;
                case "restart":
                    MessageType = MsgType.Restart;
                    break;
           
                    break;
                default:
                    MessageType = MsgType.BroadCast;
                    break;
            }
        }

    //public string CommandText { get; set; }

    //    public string CommandArgs { get; set; }
    //    public string ToUser { get; set; }
 
        ///// <summary>
        ///// Done Date
        ///// </summary>
      //  public DateTime DoneDate { get; set; }

        ///// <summary>
        ///// 創建人
        ///// </summary>
        [EntityAttribute(EntityType.Operation, "NM3.createuser")]
        public string CreateUser { get; set; }
        ///// <summary>
        ///// 創建時間
        ///// </summary>
        [EntityAttribute(EntityType.Operation, "NM3.createtime")]
        public DateTime CreateTime { get; set; }
        ///// <summary>
        ///// 最後修改人
        ///// </summary>
        [EntityAttribute(EntityType.Operation, "NM3.lastactionuser")]
        public string LastActionUser { get; set; }
        ///// <summary>
        ///// 最後修改時間
        ///// </summary>
        [EntityAttribute(EntityType.Operation, "NM3.lastactiontime")]
        public DateTime LastActionTime { get; set; }
        ///// <summary>
        ///// 最後修改資訊
        ///// </summary>
        [EntityAttribute(EntityType.Operation, "NM3.lastactioncode")]
        public string LastActionCode { get; set; }
        ///// <summary>
        ///// 状态
        ///// </summary>
        public string Status { get; set; }
        ///// 並發對象屬性
        public string OldLastActionUser { get; set; }
        public string OldLastActionCode { get; set; }
        public DateTime OldLastActionTime { get; set; }
        #endregion
        #region ILog field
        public string UIObjectCode => "SystemManage.Broadcast";   
        public string TableName => "SysData.dbo.Broadcast";
        public string ObjectInstanceKey { get { return AutoId.ToString(); } }
        public LOGTYPE LogType => LOGTYPE.MASTER;
        public string LogSqlStr { get; set; }
        public string LogDescription { get; set; }
        public object this[string key]
        {
            get
            {
                switch (key.ToLower())
                {
                    case "autoid": return AutoId;
                  
                    case "commandtext": return CommandText;
                    case "messagetype": return MessageType;
                    case "commandargs": return CommandArgs;
                    case "touser": return ToUser;
                    
                    case "createuser": return CreateUser;
                    case "createtime": return CreateTime;
                    case "lastactionuser": return LastActionUser;
                    case "lastactiontime": return LastActionTime;
                    case "lastactioncode": return LastActionCode;
                    case "status": return Status;
                    default: return string.Empty;
                }
            }
        }
        #endregion
    }
    public enum MsgType
    {
        BroadCast = 1,
        Command = 2,
        Upgrade = 3,
        None = 4,
        Lock = 5, //CommandText=UserCode
        ClearClientCache = 6,
        ClearServerCache = 7,
        Restart = 8
    }
    public class BroadcastMessage
    {
        public int AutoId { get; set; }

        public MsgType MessageType { get; set; }

        public string CommandText { get; set; }

        public string CommandArgs { get; set; }
        public string ToUser { get; set; }

        ///// <summary>
        ///// Done Date
        ///// </summary>
        public DateTime DoneDate { get; set; }
    }
}