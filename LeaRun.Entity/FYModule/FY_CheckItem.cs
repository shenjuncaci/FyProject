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
    [Description("计划项基础数据明细")]
    [PrimaryKey("ProcessID")]
    public class FY_CheckItem : BaseEntity
    {

        [DisplayName("主键")]
        public string CheckID { get; set; }

        public string ProcessID { get; set; }

        public string CheckName { get; set; }

        public int SortNO { get; set; }



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CheckID = CommonHelper.GetGuid;
            
            //this.CreateBy
        }
        
        #endregion
    }
}