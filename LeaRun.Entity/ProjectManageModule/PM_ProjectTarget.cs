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
    [Description("项目指标表")]
    [PrimaryKey("ProjectTargetID")]
    public class PM_ProjectTarget : BaseEntity
    {

        [DisplayName("主键")]
        public string ProjectTargetID { get; set; }

        public string ProjectID { get; set; }

        public string TargetContent { get; set; }
        public string BaseNum { get; set; }
        public string TargetNum { get; set; }
        public string Remark { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectTargetID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectTargetID = KeyValue;
        }

        #endregion
    }
}
