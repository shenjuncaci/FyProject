//=====================================================================================
// All Rights Reserved , Copyright @ Learun 2014
// Software Developers @ Learun 2014
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
    [Description("实际业务流程表")]
    [PrimaryKey("FlowID")]
    public class Base_FlowLog : BaseEntity
    {
        #region 获取/设置 字段值
        
        [DisplayName("主键")]
        public string FlowID { get; set; }
       
        public string FlowNo { get; set; }

        public string NoteID { get; set; }

        public int Approvestatus { get; set; }

        public string CurrentPost { get; set; }

        public string CurrentPerson { get; set; }

        public string ApprovestatusCN { get; set; }

        #endregion

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.FlowID = CommonHelper.GetGuid;
           
        }
        /// <summary>
        /// 编辑调用
        /// </summary>
        /// <param name="KeyValue"></param>
        public override void Modify(string KeyValue)
        {
            this.FlowID = KeyValue;

        }
        #endregion
    }
}