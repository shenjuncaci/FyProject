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
    [Description("宿舍表")]
    [PrimaryKey("DormID")]
    public class DM_Dorm : BaseEntity
    {

        [DisplayName("主键")]
        public string DormID { get; set; }

        public string DormName { get; set; }

        public string Remark { get; set; }

        public int Enable { get; set; }



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.DormID = CommonHelper.GetGuid;
            this.Enable = 1;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.DormID = KeyValue;

        }

        #endregion
    }
}