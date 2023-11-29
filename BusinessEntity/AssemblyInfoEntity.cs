/*
* $Author:dagger.li
* $Date:2023/7/10 上午 11:30:03
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

    public class AssemblyInfoEntity
    {
        public AssemblyInfoEntity()
        {
        }
        #region public field
        public decimal Version { get; set; }
        public string AssemblyName { get; set; }
        public string AssemblyPath { get; set; }
        public long FileDate { get; set; }
        public bool isDeleted { get; set; }

        // Ultra Grid Detail Edit Status
        public string sta { get; set; }

        #endregion

    }
}