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
    [Description("岗位验证主表")]
    [PrimaryKey("VerifyPostID")]
    public class VP_VerifyPost : BaseEntity
    {

        [DisplayName("主键")]
        public string VerifyPostID { get; set; }

        public string SetReason { get; set; }

        public string SetDepart { get; set; }

        public string SetProcess { get; set; }

        public string VerifyProduct { get; set; }

        public string VerifyDefect { get; set; }

        public DateTime? StartDate { get; set; }
        public int VerifyCycle { get; set; }
        public string VerifyMethod { get; set; }
        public string QuitStandard { get; set; }
        public int OneLevelAlarm { get; set; }
        public int TwoLevelAlarm { get; set; }
        public int ThreeLevelAlarm { get; set; }
        public string CreateBy { get; set; }
        public string Status { get; set; }
        public DateTime? RealQuitDate { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.VerifyPostID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.StartDate = DateTime.Now;
            this.Status = "进行中";
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.VerifyPostID = KeyValue;


        }

        #endregion
    }
}