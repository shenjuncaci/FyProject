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
    [Description("房费调整表")]
    [PrimaryKey("CostAdjustID")]
    public class DM_CostAdjust : BaseEntity
    {

        [DisplayName("主键")]
        public string CostAdjustID { get; set; }
        public DateTime? AdjustDate { get; set; }
        public decimal AdjustNum { get; set; }
       
        public string UserID { get; set; }
        public string Remark { get; set; }




        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CostAdjustID = CommonHelper.GetGuid;
            
        }

        public override void Modify(string KeyValue)
        {
            this.CostAdjustID = KeyValue;

        }

        #endregion
    }
}