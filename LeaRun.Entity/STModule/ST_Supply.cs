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
    [Description("供应商表")]
    [PrimaryKey("SupplyID")]
    public class ST_Supply : BaseEntity
    {

        [DisplayName("主键")]
        public string SupplyID { get; set; }
        public string SupplyName { get; set; }
        public string Remark { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.SupplyID = CommonHelper.GetGuid;
            
        }

        public override void Modify(string KeyValue)
        {
            this.SupplyID = KeyValue;

        }

        #endregion
    }
}