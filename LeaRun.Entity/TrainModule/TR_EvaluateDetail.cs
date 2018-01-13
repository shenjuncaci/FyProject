//=====================================================================================
// created by shenjun 201706019
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
    ///		<name>she</name>
    ///		<date>2014.08.11 15:45</date>
    /// </author>
    /// </summary>
    [Description("主管评审表")]
    [PrimaryKey("EvaluateID")]
    public class TR_EvaluateDetail : BaseEntity
    {

        [DisplayName("主键")]
        public string EvaluateID { get; set; }

        public string UserPostRelationID { get; set; }

        public string SkillID { get; set; }

        public string SkillName { get; set; }

        public int EvaluateScore { get; set; }
        
        public int SkillWeight { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.EvaluateID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.EvaluateID = KeyValue;
        }

        #endregion
    }
}