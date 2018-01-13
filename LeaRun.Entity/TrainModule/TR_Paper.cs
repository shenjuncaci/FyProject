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
    [Description("考卷主表")]
    [PrimaryKey("PaperID")]
    public class TR_Paper : BaseEntity
    {

        [DisplayName("主键")]
        public string PaperID { get; set; }

        public string KnowledgeBaseID { get; set; }

        public string UserID { get; set; }

        public DateTime? PaperDate { get; set; }

        public string Score { get; set; }

        public int FromSource { get; set; }

        public string SkillID { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.PaperID = CommonHelper.GetGuid;
            this.UserID = ManageProvider.Provider.Current().UserId;
            this.PaperDate = DateTime.Now;
            //this.CreateBy
        }


        public void MobileCreate()
        {
            this.PaperID = CommonHelper.GetGuid;
            //this.UserID = ManageProvider.Provider.Current().UserId;
            this.PaperDate = DateTime.Now;
        }

        public override void Modify(string KeyValue)
        {
            this.PaperID = KeyValue;
            

        }

        #endregion
    }
}