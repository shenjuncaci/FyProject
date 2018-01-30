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
    [Description("培训系统技能表")]
    [PrimaryKey("SkillID")]
    public class TR_Skill : BaseEntity
    {

        [DisplayName("主键")]
        public string SkillID { get; set; }

        public string SkillName { get; set; }

        public string SkillDescripe { get; set; }
        public string DepartmentID { get; set; }

        public string SkillType { get; set; }
        public int IsExam { get; set; }
        
        public int ExamPer { get; set; }
        public int EvaluationPer { get; set; }

        public string VideoSrc { get; set; }
        public int ExamMinutes { get; set; }
        public int QuestionNum { get; set; }
        public int IsAudit { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDt { get; set; }
        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.SkillID = CommonHelper.GetGuid;
            this.DepartmentID = ManageProvider.Provider.Current().DepartmentId;
            this.IsAudit = 0;
            this.CreateBy = ManageProvider.Provider.Current().UserName;
            this.CreateDt = DateTime.Now;
        }

        public override void Modify(string KeyValue)
        {
            this.SkillID = KeyValue;
        }

        #endregion
    }
}