//=====================================================================================
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
    [Description("判断题题库")]
    [PrimaryKey("QuestionID")]
    public class TR_JudgmentQuestion : BaseEntity
    {

        [DisplayName("主键")]
        public string QuestionID { get; set; }

        public string QuestionDescripe { get; set; }

        public string SkillID { get; set; }

        public string QuestionType { get; set; }
        
        public string Answer { get; set; }

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