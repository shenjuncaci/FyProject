﻿//=====================================================================================
// created by shenjun 201706019
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    /// <summary>
    /// 用户管理
    /// <author>
    ///		<name>she</name>
    ///		<date>2014.08.11 15:45</date>
    /// </author>
    /// </summary>
    [Description("培训项目人员表")]
    [PrimaryKey("ProjectMemberID")]
    public class TR_ProjectMember : BaseEntity
    {

        [DisplayName("主键")]
        public string ProjectMemberID { get; set; }
        public string ProjectID { get; set; }

        public string UserID { get; set; }



        



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectMemberID = CommonHelper.GetGuid;

        }

        public override void Modify(string KeyValue)
        {
            this.ProjectMemberID = KeyValue;
        }

        #endregion
    }
}