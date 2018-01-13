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
    [Description("一般问题表")]
    [PrimaryKey("GeneralProblemID")]
    public class FY_GeneralProblem : BaseEntity
    {

        [DisplayName("主键")]
        public string GeneralProblemID { get; set; }

        public string ProductArea { get; set; }
        public string ProblemType { get; set; }
        public string IsAgain { get; set; }
        public string ProblemType2 { get; set; }
        public string ResponseBy { get; set; }
        public string Customer { get; set; }
        public string ProblemDescripe { get; set; }
        public DateTime? HappenDate { get; set; }
        public string CorrectMeasures { get; set; }
        public string CauseAnalysis { get; set; }
        public string ProblemAttach { get; set; }
        public string MeasureAttach { get; set; }
        public string CauseAnalysisAttach { get; set; }
        public string FinishStatus { get; set; }
        public DateTime? RealFinshDt { get; set; }
        public DateTime? PlanFinishDt { get; set; }

        public string ImproveReport { get; set; }
        public string ImproveReportAttach { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.GeneralProblemID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.GeneralProblemID = KeyValue;

        }

        #endregion
    }
}