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
    [Description("宿舍报修表")]
    [PrimaryKey("RoomRepairID")]
    public class DM_RoomRepair : BaseEntity
    {

        [DisplayName("主键")]
        public string RoomRepairID { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string MobilePhone { get; set; }
        public string DormID { get; set; }
        public string RoomNO { get; set; }
        public string RepairPosition { get; set; }
        public string RepairProject { get; set; }
        public string IsAgree { get; set; }
        public DateTime? RepairDate { get; set; }
        public string RepairDescripe { get; set; }
        public string RepairState { get; set; }
        public decimal RepairAmount { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDt { get; set; }
        public string FlowID { get; set; }
        public string Remark { get; set; }
        public DateTime? FinishDt { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.RoomRepairID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserName;
            this.CreateDt = DateTime.Now;
            this.RepairState = "维修中";
            
        }

        public override void Modify(string KeyValue)
        {
            this.RoomRepairID = KeyValue;

        }

        #endregion
    }
}