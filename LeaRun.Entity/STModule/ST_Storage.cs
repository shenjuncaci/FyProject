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
    [Description("库存")]
    [PrimaryKey("StorageID")]
    public class ST_Storage : BaseEntity
    {

        [DisplayName("主键")]
        public string StorageID { get; set; }
        public string ProductID { get; set; }
        public int Num { get; set; }
       



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.StorageID = CommonHelper.GetGuid;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.StorageID = KeyValue;

        }

        #endregion
    }
}