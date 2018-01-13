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
    [Description("培训系统部门岗位对应关系表")]
    [PrimaryKey("RelationID")]
    public class TR_PostDepartmentRelation : BaseEntity
    {

        [DisplayName("主键")]
        public string RelationID { get; set; }

        public string PostID { get; set; }

        public string DepartmentID { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.RelationID = CommonHelper.GetGuid;
            
        }

        public override void Modify(string KeyValue)
        {
            this.RelationID = KeyValue;
        }

        #endregion
    }
}