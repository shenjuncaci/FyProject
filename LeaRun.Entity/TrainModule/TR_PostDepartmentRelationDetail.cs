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
    ///		<name>shenjun</name>
    ///		<date>2017.09.14 15:45</date>
    /// </author>
    /// </summary>
    [Description("培训系统部门岗位对应关系明细表，包含了权重等")]
    [PrimaryKey("RelationDID")]
    public class TR_PostDepartmentRelationDetail : BaseEntity
    {

        [DisplayName("主键")]
        public string RelationDID { get; set; }
        public string RelationID { get; set; }

        public string SkillID { get; set; }
        public string SkillName { get; set; }

        public string DepartmentID { get; set; }

        public int SkillWeight { get; set; }
        public int SkillRequire { get; set; }
        public int SortNo { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.RelationDID = CommonHelper.GetGuid;
            this.DepartmentID = ManageProvider.Provider.Current().DepartmentId;

        }

        public override void Modify(string KeyValue)
        {
            this.RelationDID = KeyValue;
        }

        #endregion
    }
}