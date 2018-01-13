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
    [Description("自定义考试")]
    [PrimaryKey("CustomExamID")]
    public class TR_CustomExam : BaseEntity
    {

        [DisplayName("主键")]
        public string CustomExamID { get; set; }

        public string CustomeExamName { get; set; }

        public string CreateBy { get; set; }

        public string CreateDept { get; set; }
        public DateTime? CreateDt { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public int ChoiceQuestion { get; set; }
        public int ExamMinutes { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CustomExamID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDept = ManageProvider.Provider.Current().DepartmentId;
            this.CreateDt = DateTime.Now;
           

        }

        public override void Modify(string KeyValue)
        {
            this.CustomExamID = KeyValue;
        }

        #endregion
    }
}