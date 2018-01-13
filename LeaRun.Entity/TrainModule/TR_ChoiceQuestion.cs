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
    [Description("选择题题库")]
    [PrimaryKey("QuestionID")]
    public class TR_ChoiceQuestion : BaseEntity
    {

        [DisplayName("主键")]
        public string QuestionID { get; set; }

        public string QuestionDescripe { get; set; }

        public string SkillID { get; set; }

        public string QuestionType { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Option5 { get; set; }
        public string Option6 { get; set; }
        public string Answer { get; set; }
        public int SortNO { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.QuestionID = CommonHelper.GetGuid;

        }

        public override void Modify(string KeyValue)
        {
            this.QuestionID = KeyValue;
        }

        #endregion
    }
}