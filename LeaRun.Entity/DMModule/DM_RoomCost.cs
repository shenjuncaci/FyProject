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
    [Description("房间费用表")]
    [PrimaryKey("RoomCostID")]
    public class DM_RoomCost : BaseEntity
    {

        [DisplayName("主键")]
        public string RoomCostID { get; set; }
        public string DormID { get; set; }

        public string RoomNO { get; set; }
        public string InputDate { get; set; }
        public decimal PreWater { get; set; }
        public decimal NowWater { get; set; }
        public decimal PreElectric { get; set; }
        public decimal NowElectric { get; set; }
        public decimal WaterPrice { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPriceAll { get; set; }
        public decimal ElectricPriceAll { get; set; }
        public decimal TVFee { get; set; }
        public decimal PropertyFee { get; set; }
        public decimal Rent { get; set; }
        public decimal MaintenanceFee { get; set; }

        public string Remark { get; set; }

        public string CreateBy { get; set; }
        public DateTime? CreateDt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyDt { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }





        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.RoomCostID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDt = DateTime.Now;
        }

        public override void Modify(string KeyValue)
        {
            this.RoomCostID = KeyValue;
            this.ModifyBy = ManageProvider.Provider.Current().UserId;
            this.ModifyDt = DateTime.Now;

        }

        #endregion
    }
}