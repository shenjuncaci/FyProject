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
    [PrimaryKey("AssetID")]
    public class DM_Assets : BaseEntity
    {

        [DisplayName("主键")]
        public string AssetID { get; set; }
        public string RoomID { get; set; }
        public string AssetName { get; set; }
        public int Amount { get; set; }
        public string Remark { get; set; }
        public int IsUse { get; set; }
      
        public int SortNO { get; set; }




        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.AssetID = CommonHelper.GetGuid;
            this.IsUse = 1;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.AssetID = KeyValue;

        }

        #endregion
    }
}