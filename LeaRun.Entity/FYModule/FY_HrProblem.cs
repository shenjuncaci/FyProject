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
    [Description("HR问题录入")]
    [PrimaryKey("ProblemID")]
    public class FY_HrProblem : BaseEntity
    {

        [DisplayName("主键")]
        public string ProblemID { get; set; }

        public string ProblemDescripe { get; set; }
        public string ProblemAction { get; set; }
        public string ResponseBy { get; set; }

        public string ResponseByName { get; set; }
        public string CreateBy { get; set; }
        public string CreateByName { get; set; }
        public DateTime? CreateDt { get; set; }
        public DateTime? PlanDt { get; set; }
        public DateTime? RealDt { get; set; }

        public string ProblemState { get; set; }
        
        public string AttachPath { get; set; }

        public string ProblemType { get; set; }
        public string ProblemTypeD { get; set; }

        public string FlowID { get; set; }
        public int Approvestatus { get; set; }
        public string CompleteRate { get; set; }
        public string ProposeMan { get; set; }
        public string ProposeManCode { get; set; }
        public string ProposeDept { get; set; }

        public string ProblemAttach { get; set; }

        public DateTime? ReplyDt { get; set; }

        public DateTime? RealReplyDt { get; set; }
        public string Remark { get; set; }

        public string ReplyCompleteRate { get; set; }

        public string MobilePhone { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProblemID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDt = DateTime.Now;
            this.ProblemState = "进行中";
            this.Approvestatus = 0;
            //this.CreateBy
        }

        public void MobileCreate()
        {
            this.ProblemID = CommonHelper.GetGuid;
            this.CreateDt = DateTime.Now;
            this.ProblemState = "进行中";
            this.Approvestatus = 0;
            this.CreateBy = "移动端创建";
        }

        public override void Modify(string KeyValue)
        {
            this.ProblemID = KeyValue;

        }

        #endregion
    }
}