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
    [Description("项目管理项目问题跟踪表")]
    [PrimaryKey("ProjectProblemID")]
    public class PM_ProjectProblem : BaseEntity
    {
        [DisplayName("主键")]
        public string ProjectProblemID { get; set; }

        public string ProjectID { get; set; }

        public string ProblemDescripe { get; set; }

        public string PutBy { get; set; }

        public string ResponseBy { get; set; }

        public DateTime? PutDate { get; set; }
        public DateTime? PlanDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string FinishState { get; set; }

        public string Remark { get; set; }
        public int SortNO { get; set; }

        public string Solution { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectProblemID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectProblemID = KeyValue;

        }

        #endregion
    }
}
