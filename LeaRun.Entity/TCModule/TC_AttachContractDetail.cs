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
    [Description("附件合同明细表")]
    [PrimaryKey("ContractDID")]
    public class TC_AttachContractDetail : BaseEntity
    {

        [DisplayName("主键")]
        public string ContractDID { get; set; }
        public string ContractID { get; set; }

        public string InstallType { get; set; }
        public decimal AttachNum { get; set; }
        public decimal AttachCost { get; set; }
        public decimal PlanCost { get; set; }
        public string TrueMaterialNO { get; set; }

        public int SortNO { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ContractDID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ContractDID = KeyValue;

        }

        #endregion
    }
}