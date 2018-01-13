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
    [Description("变更管理断点明细表")]
    [PrimaryKey("ChangeBreakID")]
    public class FY_ChangeBreak : BaseEntity
    {

        [DisplayName("主键")]
        public string ChangeBreakID { get; set; }

        public string ChangeBreakName { get; set; }

        public string ChangeBreakBy { get; set; }

        public string ChangeBreakByID { get; set; }
        public string StockMessage { get; set; }
        public string CreateOpinion { get; set; }

        public string ChangeID { get; set; }

        public string BreakState { get; set; }
        public string ChangeBreakDeptID { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ChangeBreakID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ChangeBreakID = KeyValue;

        }

        #endregion
    }
}