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
        public string FirstMonth { get; set; }
        public string TwoMonth { get; set; }
        public string ThreeMonth { get; set; }
        public string FourMonth { get; set; }
        public string FiveMonth { get; set; }
        public string SixMonth { get; set; }
        public string SevenMonth { get; set; }
        public string EightMonth { get; set; }
        public string NineMonth { get; set; }
        public string TenMonth { get; set; }
        public string ElevenMonth { get; set; }
        public string TwoleveMonth { get; set; }

        public string FirstMonthProfit { get; set; }
        public string TwoMonthProfit { get; set; }
        public string ThreeMonthProfit { get; set; }
        public string FourMonthProfit { get; set; }
        public string FiveMonthProfit { get; set; }
        public string SixMonthProfit { get; set; }
        public string SevenMonthProfit { get; set; }
        public string EightMonthProfit { get; set; }
        public string NineMonthProfit { get; set; }
        public string TenMonthProfit { get; set; }
        public string ElevenMonthProfit { get; set; }
        public string TwoleveMonthProfit { get; set; }

        public string NumUnit { get; set; }


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
