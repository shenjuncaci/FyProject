//=====================================================================================
// created by shenjun 20180315
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("项目管理项目人员表")]
    [PrimaryKey("ProjectMemberID")]
    public class PM_ProjectMember : BaseEntity
    {

        [DisplayName("主键")]
        public string ProjectMemberID { get; set; }

        public string ProjectID { get; set; }

        public string UserID { get; set; }

        public string PostName { get; set; }

        public string Duty { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectMemberID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectMemberID = KeyValue;

        }

        #endregion
    }
}
