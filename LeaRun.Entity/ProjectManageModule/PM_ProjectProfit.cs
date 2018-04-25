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
    [Description("项目管理项目收益表")]
    [PrimaryKey("ProjectProfitID")]
    public class PM_ProjectProfit : BaseEntity
    {
        [DisplayName("主键")]
        public string ProjectProfitID { get; set; }

        public string ProjectID { get; set; }

        public decimal ProfitNum { get; set; }
        public DateTime? ProfitDate { get; set; }
        public string Remark { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProjectProfitID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.ProjectProfitID = KeyValue;

        }

        #endregion
    }
}
