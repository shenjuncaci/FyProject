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
    [Description("房间表")]
    [PrimaryKey("RoomID")]
    public class DM_Room : BaseEntity
    {

        [DisplayName("主键")]
        public string RoomID { get; set; }
        public string DormID { get; set; }

        public string RoomNO { get; set; }
        public string RoomType { get; set; }
        public int StandardPeople { get; set; }

        public string Remark { get; set; }

        public int Enable { get; set; }



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.RoomID = CommonHelper.GetGuid;
            this.Enable = 1;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.RoomID = KeyValue;

        }

        #endregion
    }
}