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
    [Description("岗位验证明细表")]
    [PrimaryKey("VerifyPostDID")]
    public class VP_VerifyPostDetail : BaseEntity
    {

        [DisplayName("主键")]
        public string VerifyPostDID { get; set; }
        public string VerifyPostID { get; set; }
        public DateTime? VerifyDate { get; set; }
        public int DefectNum1 { get; set; }
        public int CheckNum1 { get; set; }
        public int DefectNum2 { get; set; }
        public int CheckNum2 { get; set; }

        public int DefectNum3 { get; set; }
        public int CheckNum3 { get; set; }
        public int DefectNum4 { get; set; }
        public int CheckNum4 { get; set; }
        public int DefectNum5 { get; set; }
        public int CheckNum5 { get; set; }
        public int DefectNum6 { get; set; }
        public int CheckNum6 { get; set; }

        public string QualityApprove { get; set; }
        public string FactoryManager { get; set; }
        public string WorkShopManager { get; set; }
        public string GroupManager { get; set; }
        public string Status1 { get; set; }
        public string Status2 { get; set; }
        public string Status3 { get; set; }
        public string Status4 { get; set; }
        public string Status5 { get; set; }
        public string Status6 { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.VerifyPostDID = CommonHelper.GetGuid;
            this.QualityApprove = "未完成";
            this.FactoryManager = "未完成";
            this.WorkShopManager = "未完成";
            this.GroupManager = "未完成";
        }

        public override void Modify(string KeyValue)
        {
            this.VerifyPostDID = KeyValue;


        }

        #endregion
    }
}