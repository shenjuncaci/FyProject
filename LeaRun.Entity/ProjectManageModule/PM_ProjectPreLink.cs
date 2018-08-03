using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("甘特图前置任务表")]
    [PrimaryKey("PreUID")]
    public class PM_ProjectPreLink : BaseEntity
    {

        [DisplayName("主键")]
        public string PredecessorUID { get; set; }
        public int Type { get; set; }
        public int LinkLag { get; set; }

        public string TaskUID { get; set; }

        public virtual bool Limit { get; set; }
        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            //this.PreUID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            //this.PreUID = KeyValue;

        }

        #endregion
    }
}
