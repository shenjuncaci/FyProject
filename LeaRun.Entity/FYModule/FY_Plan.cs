//=====================================================================================
// created by shenjun 20170601
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
    [Description("计划表")]
    [PrimaryKey("PlanID")]
    public class FY_Plan : BaseEntity
    {

        [DisplayName("主键")]
        public string PlanID { get; set; }

        public string ProcessID { get; set; }

        public string UserID { get; set; }

        

        public DateTime? Plandate { get; set; }

        public string PlanContent { get; set; }

        public string DepartmentID { get; set; }

        public string ResponseByID { get; set; }
        public string BackColor { get; set; }
        public string Line { get; set; }
        public string GroupID { get; set; }
        public int IsLeave { get; set; }
        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.PlanID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.PlanID = KeyValue;

        }

        #endregion
    }
}