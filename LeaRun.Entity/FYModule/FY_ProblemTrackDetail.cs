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
    [Description("问题跟踪明细表")]
    [PrimaryKey("ProblemID")]
    public class FY_ProblemTrackDetail : BaseEntity
    {

        [DisplayName("主键")]
        public string ProblemDID { get; set; }
        public string ProblemID { get; set; }

       
        public string Progress { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDt { get; set; }
        public string Remark { get; set; }
        public DateTime? PlanDt { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProblemDID = CommonHelper.GetGuid;
            this.CreateDt = DateTime.Now;
            this.CreateBy = ManageProvider.Provider.Current().UserId;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProblemDID = KeyValue;

        }

        #endregion
    }
}