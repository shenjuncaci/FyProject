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
    [Description("考试申请表")]
    [PrimaryKey("ApplyID")]
    public class TR_ExamApply : BaseEntity
    {

        [DisplayName("主键")]
        public string ApplyID { get; set; }

        public string UserID { get; set; }

        public string ExamID { get; set; }
        public int Source { get; set; }

        public DateTime? ApplyDate { get; set; }

        public int IsOk { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ApplyID = CommonHelper.GetGuid;
            this.UserID = ManageProvider.Provider.Current().UserId;
            this.ApplyDate = DateTime.Now;
            //this.CreateBy
        }


        

        public override void Modify(string KeyValue)
        {
            this.ApplyID = KeyValue;


        }

        #endregion
    }
}