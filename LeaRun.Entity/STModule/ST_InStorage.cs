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
    [Description("入库表")]
    [PrimaryKey("InID")]
    public class ST_InStorage : BaseEntity
    {

        [DisplayName("主键")]
        public string InID { get; set; }
        public string ProductID { get; set; }
        public int InNum { get; set; }
        public string Remark { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ProductName { get; set; }
        public int ProductLevel { get; set; }




        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.InID = CommonHelper.GetGuid;
            this.CreateDate = DateTime.Now;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.InID = KeyValue;

        }

        #endregion
    }
}