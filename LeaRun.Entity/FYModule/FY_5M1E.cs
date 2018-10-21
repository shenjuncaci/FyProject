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
    [Description("5M1E变化点表")]
    [PrimaryKey("ID")]
    public class FY_5M1E : BaseEntity
    {

        [DisplayName("主键")]
        public string ID { get; set; }
        public string ProcessName { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
        public string BanGroup { get; set; }
        public string ChangePoint { get; set; }
        public string ChangeContent { get; set; }
        public string ChangeReason { get; set; }
        public string ChangeLevel { get; set; }
        public string ChangeAction { get; set; }
        public DateTime? EndDate { get; set; }
        public string EndBy { get; set; }
        public string DepartmentID { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDate = DateTime.Now;
            this.DepartmentID = ManageProvider.Provider.Current().DepartmentId;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ID = KeyValue;

        }

        #endregion
    }
}