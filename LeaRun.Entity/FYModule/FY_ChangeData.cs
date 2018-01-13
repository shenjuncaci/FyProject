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
    [Description("变更基础资料表")]
    [PrimaryKey("ChangeDataID")]
    public class FY_ChangeData : BaseEntity
    {

        [DisplayName("主键")]
        public string ChangeDataID { get; set; }

        public string ChangeData { get; set; }

        public int Enable { get; set; }
        public string DepartmentID { get; set; }
        public string ResponseID { get; set; }
        public string CreateBy { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ChangeDataID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ChangeDataID = KeyValue;

        }

        #endregion
    }
}