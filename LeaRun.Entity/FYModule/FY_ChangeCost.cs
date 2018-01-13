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
    ///		<date>2017.08.17 15:45</date>
    /// </author>
    /// </summary>
    [Description("变更费用表")]
    [PrimaryKey("CostID")]
    public class FY_ChangeCost : BaseEntity
    {

        [DisplayName("主键")]
        public string CostID { get; set; }

        public string CostType { get; set; }

        public string ChangeID { get; set; }

        public decimal CostNum { get; set; }
        public string CostCurrency { get; set; }
        public string ChargedWith { get; set; }
        public string PayWith { get; set; }
        public DateTime? PayDt { get; set; }
        public string Remark { get; set; }




        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CostID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.CostID = KeyValue;

        }

        #endregion
    }
}