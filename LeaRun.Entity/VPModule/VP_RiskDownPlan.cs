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
    [Description("风险降低实施计划")]
    [PrimaryKey("RiskDownPlanID")]
    public class VP_RiskDownPlan : BaseEntity
    {

        [DisplayName("主键")]
        public string RiskDownPlanID { get; set; }
        public string AuditProcess { get; set; }
        public string Leader { get; set; }
        public DateTime? PlanStartDt { get; set; }
        public DateTime? PlanEndDt { get; set; }
        public DateTime? RealEndDt { get; set; }
        public string FinishStatus { get; set; }
        public string Remark { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.RiskDownPlanID = CommonHelper.GetGuid;
            this.FinishStatus = "进行中";
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.RiskDownPlanID = KeyValue;


        }

        #endregion
    }
}