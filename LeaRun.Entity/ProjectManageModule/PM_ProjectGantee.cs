using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("甘特图主表")]
    [PrimaryKey("UID")]
    public class PM_ProjectGantee : BaseEntity
    {

        [DisplayName("主键")]
        public string UID { get; set; }

        public string Name { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public int Duration { get; set; }
        public int PercentComplete { get; set; }
        public int Summary { get; set; }
        public int Critical { get; set; }
        public int Milestone { get; set; }
        public string ParentID { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            //this.UID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.UID = KeyValue;

        }

        #endregion
    }
}
