using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("项目管理项目表")]
    [PrimaryKey("ProjectGanteeUserID")]
    public class G_PLM_ProjectGanteeUser : BaseEntity
    {
        [DisplayName("主键")]
        public string ProjectGanteeUserID { get; set; }

        public string ProjectID { get; set; }
        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Code { get; set; }
        public string DepartName { get; set; }

        public string UID { get; set; }
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectGanteeUserID = CommonHelper.GetGuid;
            //this.UID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectGanteeUserID = KeyValue;

        }
    }
}
