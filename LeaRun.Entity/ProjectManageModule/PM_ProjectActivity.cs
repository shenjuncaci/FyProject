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
    [Description("项目管理活动表")]
    [PrimaryKey("ProjectActivityID")]
    public class PM_ProjectActivity : BaseEntity
    {

        [DisplayName("主键")]
        public string ProjectActivityID { get; set; }

        public string ProjectID { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string ActivityContent { get; set; }
        public string Remark { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectActivityID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectActivityID = KeyValue;
        }

        #endregion
    }
}
