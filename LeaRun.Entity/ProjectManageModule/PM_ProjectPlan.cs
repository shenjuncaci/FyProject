//=====================================================================================
// created by shenjun 20180315
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("项目计划表")]
    [PrimaryKey("ProjectPlanID")]
    public class PM_ProjectPlan : BaseEntity
    {

        [DisplayName("主键")]
        public string ProjectPlanID { get; set; }

        public string ProjectID { get; set; }

        public string ProjectCycle { get; set; }

        public DateTime? PlanStartDate { get; set; }

        public DateTime? PlanEndDate { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectPlanID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectPlanID = KeyValue;
        }

        #endregion
    }
}
