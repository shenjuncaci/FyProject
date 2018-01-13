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
    ///		<name>shenjun</name>
    ///		<date>2014.08.11 15:45</date>
    /// </author>
    /// </summary>
    [Description("知识库表")]
    [PrimaryKey("KnowledgeBaseID")]
    public class TR_KnowledgeBase : BaseEntity
    {

        [DisplayName("主键")]
        public string KnowledgeBaseID { get; set; }

        public string KnowledgeName { get; set; }
        public int Hours { get; set; }
        public string Mastery { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateDt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyDt { get; set; }

        public string Attach { get; set; }

        public string SkillID { get; set; }

        public string AttachName { get; set; }

        public int ChoiceQuestion { get; set; }

        public int JudgmentQuestion { get; set; }
        public int ExamMinutes { get; set; }

        public string CreateName { get; set; }

        public string VideoSrc { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.KnowledgeBaseID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDt = DateTime.Now;
            this.CreateName = ManageProvider.Provider.Current().UserName;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.KnowledgeBaseID = KeyValue;
            this.ModifyBy = ManageProvider.Provider.Current().UserId;
            this.ModifyDt = DateTime.Now;

        }

        #endregion
    }
}