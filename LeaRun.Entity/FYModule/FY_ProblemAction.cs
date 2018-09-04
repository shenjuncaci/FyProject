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
    [Description("事物预警表")]
    [PrimaryKey("ActionID")]
    public class FY_ProblemAction : BaseEntity
    {

        [DisplayName("主键")]
        public string ActionID { get; set; }

        public string ProblemDescripe { get; set; }

        public string ActionContent { get; set; }

        public string ResponseBy { get; set; }
        
        public DateTime? Plandate { get; set; }

        public string CreateBy { get; set; }

        public string CreateByDept { get; set; }

        public string AttachPath { get; set; }

        public string ProblemState { get; set; }

        public DateTime? RealCompletedate { get; set; }

        public string CauseAnaly { get; set; }
        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ActionID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateByDept = ManageProvider.Provider.Current().DepartmentId;
            this.ProblemState = "进行中";
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ActionID = KeyValue;
            //this.CreateBy = ManageProvider.Provider.Current().UserId;
            //this.CreateByDept = ManageProvider.Provider.Current().DepartmentId;
        }

        #endregion
    }
}