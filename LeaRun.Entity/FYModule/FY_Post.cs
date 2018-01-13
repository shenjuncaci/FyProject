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
    [Description("岗位表")]
    [PrimaryKey("PostID")]
    public class FY_Post : BaseEntity
    {

        [DisplayName("主键")]
        public string PostID { get; set; }

        public string PostNO { get; set; }

        public string PostName { get; set; }

        public string DepartMentID { get; set; }



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.PostID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.PostID = KeyValue;

        }

        #endregion
    }
}