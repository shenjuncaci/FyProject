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
    [Description("计划项基础数据")]
    [PrimaryKey("ProcessID")]
    public class FY_Process : BaseEntity
    {

        [DisplayName("主键")]
        public string ProcessID { get; set; }

        public string ProcessName { get; set; }

        public int SortNO { get; set; }

        public string AbleProcess { get; set; }

        public string AuditContent { get; set; }

        public string FailureEffect { get; set; }

        public string ReactionPlan { get; set; }
        
        public string DepartmentID { get; set; }
        public string DepartmentIDtxt { get; set; }
        public string ProcessNametxt { get; set; }

        public string SourceProcessID { get; set; }

        //判断来源，最原始的快速反应中的为1，由快速反应拆分出来的为2
        public int IsRapid { get; set; }

        public DateTime? EndDate { get; set; }
        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProcessID = CommonHelper.GetGuid;
            
            //this.CreateBy
        }
        /// <summary>
        /// 编辑调用
        /// </summary>
        /// <param name="KeyValue"></param>
        public override void Modify(string KeyValue)
        {
            this.ProcessID = KeyValue;

        }
        #endregion
    }
}