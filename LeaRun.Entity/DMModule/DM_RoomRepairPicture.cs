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
    
    [Description("房间维修图片表")]
    [PrimaryKey("RoomRepairPictureID")]
    public class DM_RoomRepairPicture : BaseEntity
    {

        [DisplayName("主键")]
        public string RoomRepairPictureID { get; set; }
        public string RoomRepairID { get; set; }
        public string PictureUrl { get; set; }
       
        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.RoomRepairPictureID = CommonHelper.GetGuid;
            
        }

        public override void Modify(string KeyValue)
        {
            this.RoomRepairPictureID = KeyValue;

        }

        #endregion
    }
}