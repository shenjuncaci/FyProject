//=====================================================================================
// All Rights Reserved , Copyright @ Learun 2014
// Software Developers @ Learun 2014
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;

using System.Text;

namespace LeaRun.Entity
{
    /// <summary>
    /// 用户管理
    /// <author>
    ///		<name>shenjun</name>
    ///		<date>2017.07.5</date>
    /// </author>
    /// </summary>
    [Description("实际业务流程明细表")]
    [PrimaryKey("FlowDID")]
    public class Base_FlowLogDetail : BaseEntity
    {
        #region 获取/设置 字段值

        [DisplayName("主键")]
        public string FlowDID { get; set; }

        public string FlowID { get; set; }
        public string ApproveBy { get; set; }
        public string ApprovePost { get; set; }
        public int StepNO { get; set; }
        public int IsFinish { get; set; }
        public string Approvestatus { get; set; }

        #endregion

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.FlowDID = CommonHelper.GetGuid;

        }
        /// <summary>
        /// 编辑调用
        /// </summary>
        /// <param name="KeyValue"></param>
        public override void Modify(string KeyValue)
        {
            this.FlowDID = KeyValue;

        }
        #endregion
    }
}