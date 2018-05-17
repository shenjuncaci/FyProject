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
    [Description("项目管理每次活动成员表")]
    [PrimaryKey("ProjectActivityMemberID")]
    public class PM_ProjectActivityMember : BaseEntity
    {
        [DisplayName("主键")]
        public string ProjectActivityMemberID { get; set; }

        public string ProjectActivityID { get; set; }

        public string UserID { get; set; }

       


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectActivityMemberID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectActivityMemberID = KeyValue;

        }

        #endregion
    }
}
