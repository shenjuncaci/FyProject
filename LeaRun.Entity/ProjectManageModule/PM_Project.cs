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
    [Description("项目管理项目表")]
    [PrimaryKey("ProjectID")]
    public class PM_Project : BaseEntity
    {

        [DisplayName("主键")]
        public string ProjectID { get; set; }

        public string ProjectNO { get; set; }

        public string DepartMentID { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? PlanFinishDate { get; set; }
        public string ProjectName { get; set; }
        public string ProjectNature { get; set; }
        public string ProjectStatus { get; set; }
        public string ProjectIndicators { get; set; }
        public string BenchMark { get; set; }
        public string Target { get; set; }
        public string CalculationFormula { get; set; }
        public string DataProvider { get; set; }
        public string FlowID { get; set; }
        public int Approvestatus { get; set; }
        public string Master { get; set; }
        public string Phone { get; set; }
        public string Descripe { get; set; }

        public string ExpectedInput { get; set; }
        public string ExpectedEarnings { get; set; }
        public int Direction { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDate = DateTime.Now;
            this.Approvestatus = 0;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectID = KeyValue;

        }

        #endregion
    }
}
