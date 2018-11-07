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
    [Description("出入库明细表")]
    [PrimaryKey("DID")]
    public class ST_InOutDetail : BaseEntity
    {

        [DisplayName("主键")]
        public string DID { get; set; }
        public string MID { get; set; }
        public int InOut { get; set; }
        public string ProductID { get; set; }
        public int Num { get; set; }
        public string Remark { get; set; }




        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.DID = CommonHelper.GetGuid;
            
        }

        public override void Modify(string KeyValue)
        {
            this.DID = KeyValue;

        }

        #endregion
    }
}