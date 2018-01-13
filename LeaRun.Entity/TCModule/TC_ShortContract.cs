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
    [Description("短驳合同主表")]
    [PrimaryKey("ContractID")]
    public class TC_ShortContract : BaseEntity
    {

        [DisplayName("主键")]
        public string ContractID { get; set; }

        public string ContractName { get; set; }
        public string ContractNo { get; set; }
        public string ProduceBase { get; set; }
        public string MaterialNO { get; set; }
        public string CarType { get; set; }
        public string PartNO { get; set; }
        public string ProductName { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int EveryShelfAmount { get; set; }
        public int AveriableAmount { get; set; }
        public decimal GlassFare { get; set; }
        public decimal IronBoxReturnCost { get; set; }
        public decimal AllFare { get; set; }
        public int EveryCarGlassCost { get; set; }
        public int EveryCarIronBoxCost { get; set; }
        public string Remark { get; set; }
        public string PrintRemark { get; set; }

        public int Enable { get; set; }
        public string NewContractID { get; set; }

        public DateTime CreateDt { get; set; }
        public string Attachment { get; set; }
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

//2:30