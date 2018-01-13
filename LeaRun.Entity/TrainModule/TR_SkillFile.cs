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
    [Description("技能教材库")]
    [PrimaryKey("FileID")]
    public class TR_SkillFile : BaseEntity
    {

        [DisplayName("主键")]
        public string FileID { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string SkillID { get; set; }
        public string Icon { get; set; }
        public string FileType { get; set; }
        public string FileExtensions { get; set; }
        public string FileSize { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.FileID = CommonHelper.GetGuid;

        }

        public override void Modify(string KeyValue)
        {
            this.FileID = KeyValue;
        }

        #endregion
    }
}