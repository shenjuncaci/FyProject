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
    [Description("附件合同主表")]
    [PrimaryKey("ContractID")]
    public class TC_AttachContract : BaseEntity
    {

        [DisplayName("主键")]
        public string ContractID { get; set; }

        public string MaterialNO { get; set; }

        public string CarType { get; set; }

        public string PartNO { get; set; }

        public string ProductName { get; set; }

        public string InstallType { get; set; }

        public decimal AttachNum { get; set; }
        public decimal AttachCost { get; set; }
        public decimal PlanCost { get; set; }
        public decimal LossRate { get; set; }
        public decimal EndCost { get; set; }
        public decimal Wages1 { get; set; }
        public decimal AttendanceDays1 { get; set; }
        public int Employes1 { get; set; }
        public int Shift1 { get; set; }
        public decimal LaborCost1 { get; set; }

        public decimal Wages2 { get; set; }
        public decimal AttendanceDays2 { get; set; }
        public int Employes2 { get; set; }
        public int Shift2 { get; set; }
        public decimal LaborCost2 { get; set; }

        public decimal Wages3 { get; set; }
        public decimal AttendanceDays3 { get; set; }
        public int Employes3 { get; set; }
        public int Shift3 { get; set; }
        public decimal LaborCost3 { get; set; }
        public decimal LaborCostAll { get; set; }
        public decimal Areas { get; set; }
        public decimal Power { get; set; }
        public decimal Subsidy { get; set; }
        public decimal ManageExpense { get; set; }
        public decimal SingleEndCost { get; set; }
        public string Attachment { get; set; }
        public string Remark { get; set; }
        public DateTime? CreateDt { get; set; }
        public string TrueMaterialNO { get; set; }
        public string ContractName { get; set; }
        public string ContractNo { get; set; }
        public string PrintRemark { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ContractID = CommonHelper.GetGuid;
            this.CreateDt = DateTime.Now;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ContractID = KeyValue;

        }

        #endregion
    }
}