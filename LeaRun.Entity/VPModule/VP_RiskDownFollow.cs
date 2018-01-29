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
    [Description("风险降低跟踪表")]
    [PrimaryKey("FollowID")]
    public class VP_RiskDownFollow : BaseEntity
    {

        [DisplayName("主键")]
        public string FollowID { get; set; }

        public string HighRiskItem { get; set; }
        public int BeforeS { get; set; }
        public int BeforeO { get; set; }
        public int BeforeD { get; set; }
        public int BeforeRPN { get; set; }
        public int BeforePriorityLevel { get; set; }
        public string CauseAnaly { get; set; }
        public string ActionMeasures { get; set; }
        public string ResponseBy { get; set; }
        public DateTime? PlanFinishDt { get; set; }
        public DateTime? RealFinishDt { get; set; }
        public string IsEffective { get; set; }
        public int AfterS { get; set; }
        public int AfterO { get; set; }
        public int AfterD { get; set; }
        public int AfterRPN { get; set; }
        public int AfterPriorityLevel { get; set; }

        public string FinishState { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.FollowID = CommonHelper.GetGuid;
            this.FinishState = "进行中";
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.FollowID = KeyValue;


        }

        #endregion
    }
}