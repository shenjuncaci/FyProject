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
    [Description("培训系统基础岗位表")]
    [PrimaryKey("PostID")]
    public class TR_Post : BaseEntity
    {

        [DisplayName("主键")]
        public string PostID { get; set; }

        public string PostName { get; set; }

        public string PostType { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateDt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyDt { get; set; }



        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.PostID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDt = DateTime.Now;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.PostID = KeyValue;
            this.ModifyBy = ManageProvider.Provider.Current().UserId;
            this.ModifyDt = DateTime.Now;

        }

        #endregion
    }
}