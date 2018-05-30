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
    [Description("人员入住记录表")]
    [PrimaryKey("CheckInID")]
    public class DM_CheckIn : BaseEntity
    {

        [DisplayName("主键")]
        public string CheckInID { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int IsLeave { get; set; }
        public string PersonCode { get; set; }
        public string PersonName { get; set; }
        public string PersonSex { get; set; }
        public string PersonDept { get; set; }
        public string RoomID { get; set; }
        public string Telpone {get;set;}



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CheckInID = CommonHelper.GetGuid;
            
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.CheckInID = KeyValue;

        }

        #endregion
    }
}