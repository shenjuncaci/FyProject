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
    [Description("生产线表")]
    [PrimaryKey("LineID")]
    public class FY_ProduceLine : BaseEntity
    {

        [DisplayName("主键")]
        public string LineID { get; set; }

        public string LineName { get; set; }

        public string GroupID { get; set; }
        public string DepartmentID { get; set; }



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.LineID = CommonHelper.GetGuid;
            this.DepartmentID = ManageProvider.Provider.Current().DepartmentId;
            
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.LineID = KeyValue;

        }

        #endregion
    }
}