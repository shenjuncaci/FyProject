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
    [Description("固定资产表")]
    [PrimaryKey("ProductID")]
    public class ST_Products : BaseEntity
    {

        [DisplayName("主键")]
        public string ProductID { get; set; }
        public int ProductLevel { get; set; }
        public string ProductName { get; set; }
        public string Remark { get; set; }
        public string ProductUnit { get; set; }
        public DateTime? CreateDate { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProductID = CommonHelper.GetGuid;
            this.CreateDate = DateTime.Now;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProductID = KeyValue;

        }

        #endregion
    }
}