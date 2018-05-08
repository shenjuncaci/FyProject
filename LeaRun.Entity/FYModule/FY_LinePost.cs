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
    [Description("产线对应岗位表")]
    [PrimaryKey("LinePostID")]
    public class FY_LinePost : BaseEntity
    {

        [DisplayName("主键")]
        public string LinePostID { get; set; }

        public string LineID { get; set; }
        public string PostID { get; set; }
        public string PostName { get; set; }





        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.LinePostID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.LinePostID = KeyValue;

        }

        #endregion
    }
}