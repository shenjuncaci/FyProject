using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("项目主表")]
    [PrimaryKey("ProjectID")]
    public class G_PLM_Project : BaseEntity
    {

        [DisplayName("主键")]
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string FlowID { get; set; }

        public int Approvestatus { get; set; }

        public string test { get; set; }

        public string ProjectNO { get; set; }
        public string ProjectDescripe { get; set; }
        public string ActivityType { get; set; }
        public string ProjectType { get; set; }
        public string DevelopCompany { get; set; }
        public string ProjectLevel { get; set; }
        public string ProductCompany { get; set; }
        public string MainCustomer { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public decimal CostBudget { get; set; }
        public string CostBudgetUnit { get; set; }
        public decimal RealCostBudget { get; set; }
        public string RealCostBudgetUnit { get; set; }
        public decimal ChangeBudget { get; set; }
        public string ChangeBudgetUnit { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectID = KeyValue;

        }

        #endregion
    }
}
