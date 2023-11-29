/*
* $Author:dagger.li
* $Date:2023/7/10 上午 11:18:26
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

    public class VersionsEntity
    {
        public VersionsEntity()
        {
        }
        #region public field
        ///// <summary>
        ///// 程序版本号/更新目录的名称
        ///// </summary>
        public decimal Version { get; set; }
        ///// <summary>
        ///// 版本序列號
        ///// </summary>
        public string VersionID { get; set; }
        public bool isLive { get; set; }
        public string VersionRemark { get; set; }
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
        public IList<AssemblyInfoEntity> AssemblyInfo { get; set; }
        ///// 並發對象屬性
        public string OldLastActionUser { get; set; }
        public string OldLastActionCode { get; set; }
        public DateTime OldLastActionTime { get; set; }
        #endregion

    }
}