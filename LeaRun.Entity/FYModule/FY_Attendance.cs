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
    [Description("快速反应考勤表")]
    [PrimaryKey("AttendanceID")]
    public class FY_Attendance : BaseEntity
    {

        [DisplayName("主键")]
        public string AttendanceID { get; set; }

        public DateTime? AttendanceDate { get; set; }

        public string AttendanceState { get; set; }

        public string UserID { get; set; }

        public string UserCode { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.AttendanceID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.AttendanceID = KeyValue;

        }

        #endregion
    }
}