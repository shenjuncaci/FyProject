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
    [Description("资产明细表")]
    [PrimaryKey("ProductDID")]
    public class ST_ProductsDetail : BaseEntity
    {

        [DisplayName("主键")]
        public string ProductDID { get; set; }
        public string ProductID { get; set; }
        public int ProductLevel { get; set; }
        public string ProductName { get; set; }
        //public string Remark { get; set; }
        public string ProductUnit { get; set; }
        public DateTime? CreateDate { get; set; }
        public int Num { get; set; }
        public string MID { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProductDID = CommonHelper.GetGuid;
            this.CreateDate = DateTime.Now;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProductDID = KeyValue;

        }

        #endregion
    }
}